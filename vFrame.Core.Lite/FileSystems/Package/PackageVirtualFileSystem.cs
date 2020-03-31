//------------------------------------------------------------
//        File:  PackageFileSystem.cs
//       Brief:  Package file system.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2020-03-11 16:36
//   Copyright:  Copyright (c) 2020, VyronLee
//============================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using vFrame.Core.Compress;
using vFrame.Core.Crypto;
using vFrame.Core.FileSystems.Adapters;
using vFrame.Core.FileSystems.Constants;
using vFrame.Core.FileSystems.Exceptions;
using vFrame.Core.Loggers;
using vFrame.Core.Profiles;

namespace vFrame.Core.FileSystems.Package
{
    internal class PackageVirtualFileSystem : VirtualFileSystem
    {
        private PackageHeader _header;
        private List<PackageBlockInfo> _blockInfos;
        private Dictionary<VFSPath, int> _fileList;

        private bool _closed;
        private bool _opened;

        private VFSPath _vpkVfsPath;

        public PackageVirtualFileSystem() : this(new FileStreamFactory_Default()) {
        }

        public PackageVirtualFileSystem(FileStreamFactory factory) {
            FileStreamFactory = factory;

            _fileList = new Dictionary<VFSPath, int>();
            _blockInfos = new List<PackageBlockInfo>();
            _opened = false;
            _closed = false;
        }

        public PackageVirtualFileSystem(PackageHeader header, List<PackageBlockInfo> blockInfos,
            Dictionary<VFSPath, int> fileList, bool closed, bool opened, VFSPath vpkVfsPath) {
            _header = header;
            _blockInfos = blockInfos;
            _fileList = fileList;
            _closed = closed;
            _opened = opened;
            _vpkVfsPath = vpkVfsPath;
        }

        public override void Open(VFSPath streamVfsPath) {
            if (_opened)
                throw new FileSystemAlreadyOpenedException();

            var vpkStream = FileStreamFactory.Create(streamVfsPath,
                FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            using (vpkStream) {
                InternalOpen(vpkStream);
            }

            _vpkVfsPath = streamVfsPath;
        }

        public override void Close() {
            if (_closed)
                throw new FileSystemAlreadyClosedException();

            _fileList.Clear();
            _blockInfos.Clear();

            _closed = true;
        }

        public override bool Exist(VFSPath relativeVfsPath) {
            return _fileList.ContainsKey(relativeVfsPath);
        }

        public override IVirtualFileStream GetStream(VFSPath fileName,
            FileMode mode = FileMode.Open,
            FileAccess access = FileAccess.ReadWrite,
            FileShare share = FileShare.Read
        ) {
            if (!Exist(fileName))
                throw new PackageFileSystemFileNotFound();

            var idx = _fileList[fileName];
            if (idx < 0 || idx >= _blockInfos.Count)
                throw new IndexOutOfRangeException($"Block count: {_blockInfos.Count}, but get idx: {idx}");

            var block = _blockInfos[idx];
            var vpkStream = FileStreamFactory.Create(_vpkVfsPath,
                FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            using (vpkStream) {
                //Logger.Info(PackageFileSystemConst.LogTag, "Read stream: {0}, size: {1:n0} bytes, compressed size: {2:n0} bytes",
                //    fileName, block.OriginalSize, block.CompressedSize);
                var stream = new PackageVirtualFileStream(vpkStream, block, access);
                if (!stream.Open())
                    throw new PackageStreamOpenFailedException();
                return stream;
            }
        }

        public override IReadonlyVirtualFileStreamRequest GetReadonlyStreamAsync(VFSPath fileName) {
            if (!Exist(fileName))
                throw new PackageFileSystemFileNotFound();

            var idx = _fileList[fileName];
            if (idx < 0 || idx >= _blockInfos.Count)
                throw new IndexOutOfRangeException($"Block count: {_blockInfos.Count}, but get idx: {idx}");

            var block = _blockInfos[idx];
            var vpkStream = FileStreamFactory.Create(_vpkVfsPath,
                FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            //Logger.Info(PackageFileSystemConst.LogTag, "Read stream async: {0}, size: {1:n0} bytes, compressed size: {2:n0} bytes",
            //    fileName, block.OriginalSize, block.CompressedSize);
            return new PackageReadonlyVirtualFileStreamRequest(vpkStream, block);
        }

        public override IList<VFSPath> List(IList<VFSPath> refs) {
            foreach (var kv in _fileList)
                refs.Add(kv.Key);
            return refs;
        }

        public override string ToString() {
            return _vpkVfsPath;
        }

        //=========================================================//
        //                         Private                         //
        //=========================================================//

        private void InternalOpen(Stream vpkStream) {
            Debug.Assert(null != vpkStream);

            PerfProfile.Start(out var id);
            PerfProfile.Pin("PackageVirtualFileSystem:ReadHeader", id);
            if (!ReadHeader(vpkStream))
                throw new PackageFileSystemHeaderDataError();
            PerfProfile.Unpin(id);

            PerfProfile.Start(out id);
            PerfProfile.Pin("PackageVirtualFileSystem:ReadFileList", id);
            if (!ReadFileList(vpkStream))
                throw new PackageFileSystemFileListDataError();
            PerfProfile.Unpin(id);

            PerfProfile.Start(out id);
            PerfProfile.Pin("PackageVirtualFileSystem:ReadBlockTable", id);
            if (!ReadBlockTable(vpkStream))
                throw new PackageFileSystemBlockTableDataError();
            PerfProfile.Unpin(id);

            _opened = true;
        }

        private bool ReadHeader(Stream vpkStream) {
            if (vpkStream.Length < PackageHeader.GetMarshalSize())
                return false;

            vpkStream.Seek(0, SeekOrigin.Begin);

            _header = new PackageHeader();
            using (var reader = new BinaryReader(vpkStream, Encoding.UTF8, true)) {
                _header.Id = reader.ReadInt64();
                _header.Version = reader.ReadInt64();
                _header.Size = reader.ReadInt64();
                _header.FileListOffset = reader.ReadInt64();
                _header.FileListSize = reader.ReadInt64();
                _header.BlockTableOffset = reader.ReadInt64();
                _header.BlockTableSize = reader.ReadInt64();
                _header.BlockOffset = reader.ReadInt64();
                _header.Reserved = reader.ReadInt64();
            }

            return ValidateHeader(_header);
        }

        private bool ReadFileList(Stream vpkStream) {
            if (vpkStream.Length < _header.FileListOffset + _header.FileListSize)
                return false;

            var idx = 0;
            var ret = new Dictionary<VFSPath, int>();

            vpkStream.Seek(_header.FileListOffset, SeekOrigin.Begin);
            using (var reader = new BinaryReader(vpkStream, Encoding.UTF8, true)) {
                var fileListBytes = reader.ReadBytes((int) _header.FileListSize);
                using (var decompressedStream = VirtualFileStreamPool.Instance().GetStream()) {
                    using (var decryptedStream = VirtualFileStreamPool.Instance().GetStream()) {
                        // 先解压
                        using (var tempStream = new MemoryStream(fileListBytes)) {
                            var compressService = CompressService.CreateCompressService(CompressType.LZMA);
                            compressService.Decompress(tempStream, decompressedStream);
                            compressService.Destroy();
                        }

                        decompressedStream.Seek(0, SeekOrigin.Begin);

                        // 再解密
                        var key = BitConverter.GetBytes(PackageFileSystemConst.FileListEncryptKey);
                        var cryptoService = CryptoService.CreateCryptoService(CryptoType.Xor);
                        cryptoService.Decrypt(decompressedStream, decryptedStream, key, key.Length);
                        cryptoService.Destroy();

                        decryptedStream.Seek(0, SeekOrigin.Begin);
                        using (var decryptedReader = new BinaryReader(decryptedStream)) {
                            while (decryptedStream.Position < decryptedStream.Length) {
                                var len = decryptedReader.ReadInt32();
                                var bytes = decryptedReader.ReadBytes(len);
                                if (bytes.Length != len) {
                                    throw new PackageStreamDataErrorException(bytes.Length, len);
                                }

                                var name = Encoding.UTF8.GetString(bytes);
                                ret.Add(name, idx++);
                            }
                        }
                    }
                }
            }

            _fileList = ret;
            return true;
        }

        private bool ReadBlockTable(Stream vpkStream) {
            if (vpkStream.Length < _header.BlockTableOffset + _header.BlockTableSize)
                return false;

            var ret = new List<PackageBlockInfo>();

            vpkStream.Seek(_header.BlockTableOffset, SeekOrigin.Begin);
            while (vpkStream.Position < _header.BlockTableOffset + _header.BlockTableSize)
                using (var reader = new BinaryReader(vpkStream, Encoding.UTF8, true)) {
                    var block = new PackageBlockInfo {
                        Flags = reader.ReadInt64(),
                        Offset = reader.ReadInt64(),
                        OriginalSize = reader.ReadInt64(),
                        CompressedSize = reader.ReadInt64(),
                        EncryptKey = reader.ReadInt64()
                    };
                    ret.Add(block);
                }

            if (vpkStream.Position != _header.BlockTableOffset + _header.BlockTableSize)
                return false;

            _blockInfos = ret;
            return true;
        }

        private static bool ValidateHeader(PackageHeader header) {
            return header.Id == PackageFileSystemConst.Id
                   && header.Version == PackageFileSystemConst.Version
                   && header.Size > PackageHeader.GetMarshalSize()
                   && header.FileListOffset >= PackageHeader.GetMarshalSize()
                   && header.BlockTableOffset >= header.FileListOffset + header.FileListSize
                   && header.BlockTableSize % PackageBlockInfo.GetMarshalSize() == 0
                   && header.BlockOffset >= header.BlockTableOffset + header.BlockTableSize
                ;
        }
    }
}