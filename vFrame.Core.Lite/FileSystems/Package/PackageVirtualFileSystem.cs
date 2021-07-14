﻿//------------------------------------------------------------
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
using System.IO;
using System.Threading;
using vFrame.Core.FileSystems.Constants;
using vFrame.Core.FileSystems.Exceptions;
using vFrame.Core.Profiles;

namespace vFrame.Core.FileSystems.Package
{
    public partial class PackageVirtualFileSystem : VirtualFileSystem, IPackageVirtualFileSystem
    {
        private PackageHeader _header;
        private readonly List<PackageBlockInfo> _blockInfos;
        private readonly List<VFSPath> _filePathList;
        private readonly Dictionary<string, int> _filePathMap;
        private readonly object _vfsMonitorLock = new object();

        private bool _closed;
        private bool _opened;
        private bool _dirty;
        private bool _leaveOpen;

        private VFSPath _vpkVfsPath;
        private Stream _vpkStream;
        private bool _openFromStream;

        public PackageVirtualFileSystem() {
            _header = new PackageHeader {
                Id = PackageFileSystemConst.Id,
                Version = PackageFileSystemConst.Version,
                TotalSize = PackageHeader.GetMarshalSize(),
                BlockTableOffset = PackageHeader.GetMarshalSize(),
                BlockTableSize = 0,
                BlockOffset = PackageHeader.GetMarshalSize(),
                BlockSize = 0,
                Reserved1 = 0,
                Reserved2 = 0,
                Reserved3 = 0
            };

            _filePathList = new List<VFSPath>();
            _filePathMap = new Dictionary<string, int>();
            _blockInfos = new List<PackageBlockInfo>();
            _openFromStream = false;
            _opened = false;
            _closed = false;
        }

        public override void Open(VFSPath fsPath) {
            if (_opened)
                throw new FileSystemAlreadyOpenedException();

            _vpkVfsPath = fsPath;
            _vpkStream = new FileStream(fsPath, FileMode.Open, FileAccess.Read);
            _openFromStream = false;

            InternalOpen();

            _vpkStream.Close();
            _vpkStream.Dispose();
            _vpkStream = null;
        }

        public void Open(Stream stream, bool leaveOpen = false) {
            if (_opened)
                throw new FileSystemAlreadyOpenedException();

            _vpkVfsPath = "(Streaming)";
            _vpkStream = stream ?? throw new ArgumentNullException(nameof(stream));
            _leaveOpen = leaveOpen;
            _openFromStream = true;

            InternalOpen();
        }

        public override void Close() {
            if (_closed)
                return;

            Flush();

            _filePathList.Clear();
            _filePathMap.Clear();
            _blockInfos.Clear();

            if (null != _vpkStream && !_leaveOpen) {
                _vpkStream.Close();
            }

            _opened = false;
            _closed = true;
        }

        public static PackageVirtualFileSystem CreatePackage(string savePath) {
            if (File.Exists(savePath)) {
                throw new FileAlreadyExistException(savePath);
            }

            var fs = new PackageVirtualFileSystem {
                _vpkVfsPath = savePath,
                _vpkStream = File.Create(savePath),
                _opened = true,
                _leaveOpen = false,
            };
            return fs;
        }

        public override bool Exist(VFSPath filePath) {
            if (!_opened) {
                throw new FileSystemNotOpenedException();
            }
            return _filePathMap.ContainsKey(filePath);
        }

        public override IVirtualFileStream GetStream(VFSPath filePath,
            FileMode mode = FileMode.Open,
            FileAccess access = FileAccess.Read,
            FileShare share = FileShare.Read
        ) {
            if (!_opened) {
                throw new FileSystemNotOpenedException();
            }

            if (!Exist(filePath))
                throw new PackageFileSystemFileNotFoundException(filePath);

            if (mode != FileMode.Open) {
                throw new FileSystemNotSupportedException("Only 'FileMode.Open'  is supported in package virtual file system.");
            }

            if (access != FileAccess.Read) {
                throw new FileSystemNotSupportedException("Only 'FileAccess.Read' is supported in package virtual file system.");
            }

            var block = GetBlockInfoInternal(filePath);
            OnGetStream?.Invoke(_vpkVfsPath, filePath, block.OriginalSize, block.CompressedSize);
            var stream = new PackageVirtualFileStream(GetVPKStream(), block);
            if (!stream.Open())
                throw new PackageStreamOpenFailedException();
            return stream;
        }

        public override IReadonlyVirtualFileStreamRequest GetReadonlyStreamAsync(VFSPath filePath) {
            if (!_opened) {
                throw new FileSystemNotOpenedException();
            }

            var block = GetBlockInfoInternal(filePath);
            OnGetStream?.Invoke(_vpkVfsPath, filePath, block.OriginalSize, block.CompressedSize);
            return new PackageReadonlyVirtualFileStreamRequest(GetVPKStream(), block);
        }

        public override IList<VFSPath> List(IList<VFSPath> refs) {
            if (!_opened) {
                throw new FileSystemNotOpenedException();
            }

            for (var i = 0; i < _filePathList.Count; i++) {
                if (IsBlockDeleted(_blockInfos[i])) {
                    continue;
                }
                if (_filePathList[i] == PackageFileSystemConst.FileListFileName) {
                    continue;
                }
                refs.Add(_filePathList[i]);
            }
            return refs;
        }

        public override event OnGetStreamEventHandler OnGetStream;

        public event OnGetPackageBlockEventHandler OnGetBlock;

        public VFSPath PackageFilePath {
            get => _vpkVfsPath;
            protected set => _vpkVfsPath = value;
        }

        public PackageBlockInfo GetBlockInfo(string filePath) {
            if (!_opened) {
                throw new FileSystemNotOpenedException();
            }

            OnGetBlock?.Invoke(_vpkVfsPath, filePath);
            return GetBlockInfoInternal(filePath);
        }

        public void AddFile(string filePath) {
            if (ReadOnly) {
                throw new FileSystemNotSupportedException("File system is readonly.");
            }
            AddFile(filePath, 0, 0, 0);
        }

        public void AddFile(string filePath, long encryptType, long encryptKey, long compressType) {
            if (ReadOnly) {
                throw new FileSystemNotSupportedException("File system is readonly.");
            }
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                AddStream(filePath, fs, encryptType, encryptKey, compressType);
            }
        }

        public void AddStream(string filePath, Stream stream) {
            if (ReadOnly) {
                throw new FileSystemNotSupportedException("File system is readonly.");
            }
            AddStream(filePath, stream, 0, 0, 0);
        }

        public void AddStream(string filePath, Stream stream, long encryptType, long encryptKey, long compressType) {
            if (ReadOnly) {
                throw new FileSystemNotSupportedException("File system is readonly.");
            }

            if (Exist(filePath)) {
                throw new FileAlreadyExistException(filePath);
            }

            var blockInfo = CalculateBlockInfo(stream, encryptType, encryptKey, compressType);
            blockInfo.OpFlags = BlockOpFlags.New;

            _blockInfos.Add(blockInfo);
            _filePathList.Add(filePath);

            Dirty();
            Rehash();
        }

        public void DeleteFile(string filePath) {
            if (ReadOnly) {
                throw new FileSystemNotSupportedException("File system is readonly.");
            }

            if (!_filePathMap.ContainsKey(filePath)) {
                return;
            }

            var idx = _filePathMap[filePath];
            if (idx >= _blockInfos.Count) {
                throw new PackageBlockIndexOutOfRangeException();
            }

            var blockInfo = _blockInfos[idx];

            if (IsBlockWrite(blockInfo)) { // If block exist(already write to disk), just mark as deleted
                blockInfo.OpFlags = BlockOpFlags.Deleted;
                _blockInfos[idx] = blockInfo;
            }
            else { // Otherwise, remove unsaved block from table
                _blockInfos.RemoveAt(idx);
                _filePathList.RemoveAt(idx);
            }

            Dirty();
            Rehash();
        }

        public void Flush(bool clean = false) {
            if (!_opened) {
                throw new FileSystemNotOpenedException();
            }

            if (!_dirty) {
                return;
            }

            if (ReadOnly) {
                throw new FileSystemNotSupportedException("File system is readonly.");
            }

            PrepareWriting(clean);
            WriteHeader();
            WriteBlockData(clean);
            WriteBlockTable(clean);
            FinishWriting();
        }

        public bool ReadOnly { get; protected set; }

        public override string ToString() {
            return _vpkVfsPath;
        }

        //=========================================================//
        //                         Private                         //
        //=========================================================//

        private void InternalOpen() {
            PerfProfile.Start(out var id);
            PerfProfile.Pin("PackageVirtualFileSystem:ReadHeader", id);
            if (!ReadHeader())
                throw new PackageFileSystemHeaderDataErrorException();
            PerfProfile.Unpin(id);

            PerfProfile.Start(out id);
            PerfProfile.Pin("PackageVirtualFileSystem:ReadBlockTable", id);
            if (!ReadBlockTable())
                throw new PackageFileSystemBlockTableDataErrorException();
            PerfProfile.Unpin(id);

            PerfProfile.Start(out id);
            PerfProfile.Pin("PackageVirtualFileSystem:ReadFileList", id);
            if (!ReadFileList())
                throw new PackageFileSystemFileListDataErrorException();
            PerfProfile.Unpin(id);

            Rehash();

            _opened = true;
            _closed = false;
        }

        private PackageBlockInfo GetBlockInfoInternal(string filePath) {
            if (!_filePathMap.ContainsKey(filePath)) {
                throw new PackageFileSystemFileNotFoundException(filePath);
            }

            var idx = _filePathMap[filePath];
            if (idx < 0 || idx >= _blockInfos.Count)
                throw new IndexOutOfRangeException($"Block count: {_blockInfos.Count}, but get idx: {idx}");

            return _blockInfos[idx];
        }

        private void Dirty() {
            if (_dirty) {
                return;
            }
            _dirty = true;

            // Mark filename list deleted, so it will be remove on save.
            DeleteFileNameListBlock();
        }

        private void DeleteFileNameListBlock() {
            if (!_filePathMap.ContainsKey(PackageFileSystemConst.FileListFileName)) {
                return;
            }
            var idx = _filePathMap[PackageFileSystemConst.FileListFileName];
            var blockInfo = _blockInfos[idx];
            blockInfo.Flags |= BlockFlags.BlockDeleted;
            _blockInfos[idx] = blockInfo;

            _filePathList.RemoveAt(idx);
            _filePathMap.Remove(PackageFileSystemConst.FileListFileName);
        }

        private void Rehash() {
            _filePathMap.Clear();

            for (var i = 0; i < _filePathList.Count; i++) {
                if (IsBlockDeleted(_blockInfos[i])) {
                    continue;
                }
                _filePathMap.Add(_filePathList[i], i);
            }
        }

        private static bool IsBlockDeleted(PackageBlockInfo blockInfo) {
            return (blockInfo.Flags & BlockFlags.BlockDeleted) > 0 || (blockInfo.OpFlags & BlockOpFlags.Deleted) > 0;
        }

        private static bool IsBlockWrite(PackageBlockInfo blockInfo) {
            return (blockInfo.Flags & BlockFlags.BlockExists) > 0;
        }

        private PackageVirtualFileSystemStream GetVPKStream() {
            if (_openFromStream) {
                return new PackageVirtualFileSystemStream(_vpkStream, this, true);
            }

            var fileStream = new FileStream(_vpkVfsPath, FileMode.Open, FileAccess.Read);
            return new PackageVirtualFileSystemStream(fileStream, this, false);
        }

        internal void Lock() {
            Monitor.Enter(_vfsMonitorLock);
        }

        internal void Unlock() {
            Monitor.Exit(_vfsMonitorLock);
        }
    }
}