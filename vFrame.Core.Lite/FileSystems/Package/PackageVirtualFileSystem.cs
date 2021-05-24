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
using vFrame.Core.Compress.Services;
using vFrame.Core.Crypto;
using vFrame.Core.FileSystems.Constants;
using vFrame.Core.FileSystems.Exceptions;
using vFrame.Core.Profiles;

namespace vFrame.Core.FileSystems.Package
{
    public class PackageVirtualFileSystem : VirtualFileSystem, IPackageVirtualFileSystem
    {
        private PackageHeader _header;
        private List<PackageBlockInfo> _blockInfos;
        private Dictionary<VFSPath, int> _fileList;

        private bool _closed;
        private bool _opened;

        private VFSPath _vpkVfsPath;
        private Stream _vpkStream;

        public PackageVirtualFileSystem() {
            _fileList = new Dictionary<VFSPath, int>();
            _blockInfos = new List<PackageBlockInfo>();
            _opened = false;
            _closed = false;
        }

        public PackageVirtualFileSystem(PackageHeader header, List<PackageBlockInfo> blockInfos,
            Dictionary<VFSPath, int> fileList, bool closed, bool opened, VFSPath vpkVfsPath) : this() {
            _header = header;
            _blockInfos = blockInfos;
            _fileList = fileList;
            _closed = closed;
            _opened = opened;
            _vpkVfsPath = vpkVfsPath;
        }

        public override void Open(VFSPath fsPath) {
            if (null == fsPath) {
                throw new ArgumentNullException(nameof(fsPath));
            }

            if (_opened)
                throw new FileSystemAlreadyOpenedException();

            _vpkVfsPath = fsPath;
            _vpkStream = new FileStream(fsPath, FileMode.Open, FileAccess.Read);

            InternalOpen(_vpkStream);
        }

        public override void Open(Stream stream) {
            if (null == stream) {
                throw new ArgumentNullException(nameof(stream));
            }

            if (_opened)
                throw new FileSystemAlreadyOpenedException();

            _vpkVfsPath = "(Streaming)";
            _vpkStream = stream;

            InternalOpen(stream);
        }

        public override void Close() {
            if (_closed)
                throw new FileSystemAlreadyClosedException();

            Flush();

            _fileList.Clear();
            _blockInfos.Clear();

            _opened = false;
            _closed = true;
        }

        public override bool Exist(VFSPath relativeVfsPath) {
            if (!_opened) {
                throw new FileSystemNotOpenedException();
            }
            return _fileList.ContainsKey(relativeVfsPath);
        }

        public override IVirtualFileStream GetStream(VFSPath fileName,
            FileMode mode = FileMode.Open,
            FileAccess access = FileAccess.Read,
            FileShare share = FileShare.Read
        ) {
            if (!_opened) {
                throw new FileSystemNotOpenedException();
            }

            if (!Exist(fileName))
                throw new PackageFileSystemFileNotFound();

            if (mode != FileMode.Open) {
                throw new NotSupportedException("Only 'FileMode.Open'  is supported in package virtual file system.");
            }

            if (access != FileAccess.Read) {
                throw new NotSupportedException("Only 'FileAccess.Read' is supported in package virtual file system.");
            }

            var block = GetBlockInfoInternal(fileName);
            OnGetStream?.Invoke(_vpkVfsPath, fileName, block.OriginalSize, block.CompressedSize);
            var stream = new PackageVirtualFileStream(_vpkStream, block);
            if (!stream.Open())
                throw new PackageStreamOpenFailedException();
            return stream;
        }

        public override IReadonlyVirtualFileStreamRequest GetReadonlyStreamAsync(VFSPath fileName) {
            if (!_opened) {
                throw new FileSystemNotOpenedException();
            }

            var block = GetBlockInfoInternal(fileName);
            OnGetStream?.Invoke(_vpkVfsPath, fileName, block.OriginalSize, block.CompressedSize);
            return new PackageReadonlyVirtualFileStreamRequest(_vpkStream, block);
        }

        public override IList<VFSPath> List(IList<VFSPath> refs) {
            if (!_opened) {
                throw new FileSystemNotOpenedException();
            }

            foreach (var kv in _fileList)
                refs.Add(kv.Key);
            return refs;
        }

        public override event OnGetStreamEventHandler OnGetStream;

        public event OnGetPackageBlockEventHandler OnGetBlock;

        public VFSPath PackageFilePath {
            get => _vpkVfsPath;
            protected set => _vpkVfsPath = value;
        }

        public PackageBlockInfo GetBlockInfo(string fileName) {
            if (!_opened) {
                throw new FileSystemNotOpenedException();
            }

            OnGetBlock?.Invoke(_vpkVfsPath, fileName);
            return GetBlockInfoInternal(fileName);
        }

        public void AddFile(string filePath) {
            AddFile(filePath, 0, 0, 0);
        }

        public void AddFile(string filePath, long encryptType, long encryptKey, long compressType) {
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            AddStream(filePath, fs, encryptType, encryptKey, compressType);
        }

        public void AddStream(string filePath, Stream stream) {
            AddStream(filePath, stream, 0, 0, 0);
        }

        public void AddStream(string filePath, Stream stream, long encryptType, long encryptKey, long compressType) {
            var path = VFSPath.GetPath(filePath);
            if (_fileList.ContainsKey(path)) {
                throw new FileAlreadyExistException(filePath);
            }

            var idx = _blockInfos.Count;
            _fileList.Add(filePath, idx);

            PackageVirtualFileOperator.GetBlockInfoAndBytes(stream,
                encryptType,
                encryptKey,
                compressType,
                out var buffer,
                out var blockInfo);
            _blockInfos.Add(blockInfo);
        }

        public void DeleteFile(string filePath) {
            if (!Exist(filePath)) {
                return;
            }

            var idx = _fileList[filePath];
            var blockInfo = _blockInfos[idx];
            blockInfo.OpFlags = BlockOpFlags.Deleted;
            _blockInfos[idx] = blockInfo;
        }

        public void Flush() {

        }

        public bool ReadOnly { get; protected set; } = false;

        private PackageBlockInfo GetBlockInfoInternal(string fileName) {
            if (!Exist(fileName))
                throw new PackageFileSystemFileNotFound();

            var idx = _fileList[fileName];
            if (idx < 0 || idx >= _blockInfos.Count)
                throw new IndexOutOfRangeException($"Block count: {_blockInfos.Count}, but get idx: {idx}");

            return _blockInfos[idx];
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
            _closed = false;
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