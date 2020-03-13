﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using vFrame.Core.Compress;
using vFrame.Core.Crypto;
using vFrame.Core.Extensions;
using vFrame.Core.FileSystems.Constants;
using vFrame.Core.FileSystems.Exceptions;
using vFrame.Core.FileSystems.Standard;

namespace vFrame.Core.FileSystems.Package
{
    public static class PackageVirtualFileOperator
    {
        public enum ProcessState
        {
            ScanningFileList,
            CalculatingBlockInfo,
            WritingHeader,
            WritingFileList,
            WritingBlockInfo,
            WritingBlockData,
        }

        public static void CreatePackage(string directory,
            string outputPath,
            Action<ProcessState, float, float> onProgress = null
        ) {
            CreatePackage(directory,
                outputPath,
                BlockFlags.BlockEncryptXor,
                PackageFileSystemConst.Id,
                BlockFlags.BlockCompressZlib,
                onProgress);
        }

        public static void CreatePackage(string directory,
            string outputPath,
            long encryptType,
            long encryptKey,
            long compressType,
            Action<ProcessState, float, float> onProgress = null
        ) {
            var stdFileSystem = new StandardVirtualFileSystem();
            stdFileSystem.Open(directory);

            var files = stdFileSystem.List();
            var fileListBytes = GetFileListBytes(files, onProgress);
            var blockInfos = GetBlockInfo(stdFileSystem,
                files,
                encryptType,
                encryptKey,
                compressType,
                out var blockBytesData,
                onProgress);

            var outputStream = new FileStream(outputPath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None);
            using (outputStream) {
                outputStream.SetLength(0);
                outputStream.Seek(0, SeekOrigin.Begin);

                var header = WriteHeader(outputStream,
                    files,
                    fileListBytes,
                    blockBytesData,
                    onProgress);
                WriteFileList(outputStream, fileListBytes, onProgress);
                WriteBlockInfo(outputStream, header, blockInfos, onProgress);
                WriteBlocks(outputStream, blockBytesData, onProgress);

                outputStream.Flush(true);
                outputStream.Close();
            }

            stdFileSystem.Close();
        }

        public static void ExtractPackage(string pkgPath,
            string destPath,
            Action<string, float, float> onProgress
        ) {
            var pkgFileSystem = new PackageVirtualFileSystem();
            pkgFileSystem.Open(pkgPath);

            var stdFileSystem = new StandardVirtualFileSystem();
            stdFileSystem.Open(destPath);

            var files = pkgFileSystem.List();
            var idx = 0;
            var total = files.Count;
            foreach (var path in files) {
                using (var input = (Stream) pkgFileSystem.GetStream(path)) {
                    var absolute = VFSPath.GetPath(destPath) + path;
                    var dirName = absolute.GetDirectoryName();
                    Directory.CreateDirectory(dirName);

                    if (stdFileSystem.Exist(path)) {
                        throw new FileAlreadyExistException(path);
                    }

                    using (var output = new FileStream(absolute.GetValue(), FileMode.Create)) {
                        input.CopyTo(output);
                    }
                }

                onProgress?.Invoke(path.GetValue(), idx++, total);
            }

            pkgFileSystem.Close();
            stdFileSystem.Close();
        }

        //======================================================//
        //                      Private                         //
        //======================================================//

        private static List<byte[]> GetFileListBytes(ICollection<VFSPath> files,
            Action<ProcessState, float, float> onProgress) {
            var ret = new List<byte[]>();
            var total = files.Count;
            var idx = 0f;
            foreach (var path in files) {
                using (var stream = new MemoryStream()) {
                    using (var writer = new BinaryWriter(stream)) {
                        writer.Write(path.GetValue());
                        ret.Add(stream.ToArray());
                    }
                }
                onProgress?.Invoke(ProcessState.ScanningFileList, idx++, total);
            }

            return ret;
        }

        private static List<PackageBlockInfo> GetBlockInfo(
            IVirtualFileSystem vfs,
            IList<VFSPath> files,
            long encryptType,
            long encryptKey,
            long compressType,
            out List<byte[]> blocksDataByte,
            Action<ProcessState, float, float> onProgress
        ) {
            blocksDataByte = new List<byte[]>();
            var ret = new List<PackageBlockInfo>();
            var total = files.Count;
            var idx = 0f;
            foreach (var path in files)
                using (var fs = (Stream) vfs.GetStream(path, FileMode.Open, FileAccess.Read)) {
                    var buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, (int) fs.Length);

                    var blockInfo = new PackageBlockInfo {
                        Flags = BlockFlags.BlockExists,
                        OriginalSize = fs.Length,
                        CompressedSize = fs.Length
                    };

                    // 启用加密
                    if (encryptType != 0) {
                        buffer = EncryptBytes(buffer, encryptType, encryptKey);
                        blockInfo.Flags |= encryptType;
                        blockInfo.EncryptKey = encryptKey;
                    }

                    // 启用压缩
                    if (compressType != 0) {
                        buffer = CompressBytes(buffer, compressType);
                        blockInfo.Flags |= compressType;
                        blockInfo.CompressedSize = buffer.Length;
                    }

                    blocksDataByte.Add(buffer);
                    ret.Add(blockInfo);

                    onProgress?.Invoke(ProcessState.CalculatingBlockInfo, idx++, total);
                }

            return ret;
        }

        private static byte[] EncryptBytes(byte[] buffer, long encryptType, long encryptKey) {
            using (var encrypted = new MemoryStream()) {
                using (var input = new MemoryStream(buffer)) {
                    var keyBytes = BitConverter.GetBytes(encryptKey);
                    var cryptoService = CryptoService.CreateCryptoService((CryptoType) (encryptType >> 12));
                    cryptoService.Encrypt(input, encrypted, keyBytes, keyBytes.Length);
                    cryptoService.Destroy();
                    return encrypted.ToArray();
                }
            }
        }

        private static byte[] CompressBytes(byte[] buffer, long compressType) {
            using (var compressed = new MemoryStream()) {
                using (var input = new MemoryStream(buffer)) {
                    var compressService = CompressService.CreateCompressService((CompressType) (compressType >> 8));
                    compressService.Compress(input, compressed);
                    compressService.Destroy();
                    return compressed.ToArray();
                }
            }
        }

        private static PackageHeader WriteHeader(Stream stream,
            IList<VFSPath> files,
            List<byte[]> fileListBytes,
            List<byte[]> blocksDataByte,
            Action<ProcessState, float, float> onProgress) {
            var header = new PackageHeader();
            header.Id = PackageFileSystemConst.Id;
            header.Version = PackageFileSystemConst.Version;
            header.FileListOffset = PackageHeader.GetMarshalSize();
            header.FileListSize = fileListBytes.Sum(bytes => bytes.Length);
            header.BlockTableOffset = header.FileListOffset + header.FileListSize;
            header.BlockTableSize = PackageBlockInfo.GetMarshalSize() * files.Count;
            header.BlockOffset = header.BlockTableOffset + header.BlockTableSize;
            header.Reserved = 0;

            var blockSize = blocksDataByte.Sum(bytes => bytes.Length);
            header.Size = PackageHeader.GetMarshalSize() * header.FileListSize + header.BlockTableSize + blockSize;

            onProgress?.Invoke(ProcessState.WritingHeader, 0, 1);
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true)) {
                writer.Write(header.Id);
                writer.Write(header.Version);
                writer.Write(header.Size);
                writer.Write(header.FileListOffset);
                writer.Write(header.FileListSize);
                writer.Write(header.BlockTableOffset);
                writer.Write(header.BlockTableSize);
                writer.Write(header.BlockOffset);
                writer.Write(header.Reserved);
            }

            onProgress?.Invoke(ProcessState.WritingHeader, 1, 1);

            return header;
        }

        private static void WriteFileList(FileStream outputStream,
            List<byte[]> fileListBytes,
            Action<ProcessState, float, float> onProgress
        ) {
            using (var writer = new BinaryWriter(outputStream, Encoding.UTF8, true)) {
                var total = fileListBytes.Count;
                var idx = 0f;

                foreach (var fileListByte in fileListBytes) {
                    writer.Write(fileListByte);
                    onProgress?.Invoke(ProcessState.WritingFileList, idx++, total);
                }
            }
        }

        private static void WriteBlockInfo(
            FileStream outputStream,
            PackageHeader header,
            List<PackageBlockInfo> blockInfos,
            Action<ProcessState, float, float> onProgress
        ) {
            var offset = header.BlockOffset;
            var total = blockInfos.Count;
            var idx = 0f;
            using (var writer = new BinaryWriter(outputStream, Encoding.UTF8, true)) {
                for (var i = 0; i < blockInfos.Count; i++) {
                    var block = blockInfos[i];
                    writer.Write(block.Flags);
                    writer.Write(offset);
                    writer.Write(block.OriginalSize);
                    writer.Write(block.CompressedSize);
                    writer.Write(block.EncryptKey);

                    offset += block.CompressedSize;

                    onProgress?.Invoke(ProcessState.WritingBlockInfo, idx++, total);
                }
            }
        }

        private static void WriteBlocks(Stream stream,
            List<byte[]> blockBytesData,
            Action<ProcessState, float, float> onProgress
        ) {
            var total = blockBytesData.Count;
            var idx = 0f;
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true)) {
                foreach (var bytes in blockBytesData) {
                    writer.Write(bytes);
                    onProgress?.Invoke(ProcessState.WritingBlockData, idx++, total);
                }
            }
        }
    }
}