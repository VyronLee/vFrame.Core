//------------------------------------------------------------
//        File:  PackageStream.cs
//       Brief:  Package stream.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2020-03-11 16:39
//   Copyright:  Copyright (c) 2020, VyronLee
//============================================================

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using vFrame.Core.Compress;
using vFrame.Core.Crypto;
using vFrame.Core.FileSystems.Constants;
using vFrame.Core.FileSystems.Exceptions;
using vFrame.Core.Profiles;

namespace vFrame.Core.FileSystems.Package
{
    internal class PackageVirtualFileStream : VirtualFileStream
    {
        private readonly PackageBlockInfo _blockInfo;
        private readonly FileAccess _mode = FileAccess.ReadWrite;

        private readonly Stream _vpkStream;
        private MemoryStream _memoryStream;
        private bool _opened;
        private bool _closed;

        internal PackageVirtualFileStream(Stream vpkStream, PackageBlockInfo blockInfo) {
            _vpkStream = vpkStream;
            _blockInfo = blockInfo;
        }

        internal PackageVirtualFileStream(Stream vpkStream, PackageBlockInfo blockInfo, FileAccess mode)
            : this(vpkStream, blockInfo) {
            _mode = mode;
        }

        public override bool CanRead => _opened && !_closed && (_mode & FileAccess.Read) > 0;
        public override bool CanSeek => _opened && !_closed;
        public override bool CanWrite => _opened && !_closed && (_mode & FileAccess.Write) > 0;
        public override long Length => _blockInfo.OriginalSize;

        public override long Position {
            get => _memoryStream.Position;
            set => _memoryStream.Position = value;
        }

        public bool Open() {
            Debug.Assert(null != _vpkStream);
            PerfProfile.Start(out var id);
            PerfProfile.Pin("PackageVirtualFileStream:InternalOpen", id);
            var ret = InternalOpen(_vpkStream);
            PerfProfile.Unpin(id);
            return ret;
        }

        public override void Close() {
            _memoryStream.Close();
            _memoryStream.Dispose();

            _closed = true;

            base.Close();
        }

        public byte[] GetBuffer() {
            ValidateStreamState();
            return _memoryStream.GetBuffer();
        }

        public byte[] ToArray() {
            ValidateStreamState();
            return _memoryStream.ToArray();
        }

        public override void Flush() {
            ValidateStreamState();
            _memoryStream.Flush();
            //TODO: Flush to package.
        }

        public override int Read(byte[] buffer, int offset, int count) {
            ValidateStreamState();
            return _memoryStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            ValidateStreamState();
            return _memoryStream.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            ValidateStreamState();
            _memoryStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            ValidateStreamState();
            _memoryStream.Write(buffer, offset, count);
        }

        //=======================================================//
        //                         Private                     ==//
        //=======================================================//

        private void ValidateStreamState() {
            if (!_opened) throw new PackageStreamNotOpenedException();
            if (_closed) throw new PackageStreamClosedException();
        }

        private bool InternalOpen(Stream inputStream) {
            ValidateBlockInfo(inputStream);

            _memoryStream = VirtualFileStreamPool.Instance().GetStream();

            using (var tempStream = VirtualFileStreamPool.Instance().GetStream()) {
                // 先解压
                if ((_blockInfo.Flags & BlockFlags.BlockCompressed) > 0) {
                    inputStream.Seek(_blockInfo.Offset, SeekOrigin.Begin);
                    using (var reader = new BinaryReader(inputStream, Encoding.UTF8, true)) {
                        using (var writer = new BinaryWriter(tempStream, Encoding.UTF8, true)) {
                            writer.Write(reader.ReadBytes((int)_blockInfo.CompressedSize));
                        }
                    }

                    PerfProfile.Start(out var id);
                    PerfProfile.Pin(
                        $"PackageVirtualFileStream:Decompress size: {_blockInfo.OriginalSize:n0} bytes: ", id);
                    using (var decompressedStream = VirtualFileStreamPool.Instance().GetStream()) {
                        var compressType = (_blockInfo.Flags & BlockFlags.BlockCompressed) >> 8;
                        var compressService = CompressService.CreateCompressService((CompressType) compressType);

                        tempStream.Seek(0, SeekOrigin.Begin);
                        compressService.Decompress(tempStream, decompressedStream);
                        compressService.Destroy();

                        tempStream.SetLength(0);
                        tempStream.Seek(0, SeekOrigin.Begin);
                        decompressedStream.Seek(0, SeekOrigin.Begin);
                        decompressedStream.CopyTo(tempStream);
                    }
                    PerfProfile.Unpin(id);
                }
                else {
                    inputStream.Seek(_blockInfo.Offset, SeekOrigin.Begin);
                    using (var reader = new BinaryReader(inputStream, Encoding.UTF8, true)) {
                        using (var writer = new BinaryWriter(tempStream, Encoding.UTF8, true)) {
                            writer.Write(reader.ReadBytes((int)_blockInfo.OriginalSize));
                        }
                    }
                }

                // 再解密
                if ((_blockInfo.Flags & BlockFlags.BlockEncrypted) > 0) {
                    PerfProfile.Start(out var id);
                    PerfProfile.Pin(
                        $"PackageVirtualFileStream:Decrypt size: {_blockInfo.OriginalSize:n0} bytes", id);
                    using (var decryptedStream = VirtualFileStreamPool.Instance().GetStream()) {
                        var cryptoKey = BitConverter.GetBytes(_blockInfo.EncryptKey);
                        var cryptoType = (_blockInfo.Flags & BlockFlags.BlockEncrypted) >> 12;
                        var cryptoService = CryptoService.CreateCryptoService((CryptoType) cryptoType);

                        tempStream.Seek(0, SeekOrigin.Begin);
                        cryptoService.Decrypt(tempStream, decryptedStream, cryptoKey, cryptoKey.Length);
                        cryptoService.Destroy();

                        decryptedStream.Seek(0, SeekOrigin.Begin);
                        decryptedStream.CopyTo(_memoryStream);
                    }
                    PerfProfile.Unpin(id);
                }
                else {
                    tempStream.Seek(0, SeekOrigin.Begin);
                    tempStream.CopyTo(_memoryStream);
                }
            }

            if (_memoryStream.Length != _blockInfo.OriginalSize) {
                throw new PackageStreamDataLengthNotMatchException(_memoryStream.Length, _blockInfo.OriginalSize);
            }
            _memoryStream.Seek(0, SeekOrigin.Begin);
            _memoryStream.SetLength(_blockInfo.OriginalSize);

            return _opened = true;
        }

        private void ValidateBlockInfo(Stream inputStream) {
            if ((_blockInfo.Flags & BlockFlags.BlockExists) <= 0)
                throw new PackageBlockDisposedException();

            var sizeOfHeader = PackageHeader.GetMarshalSize();
            if (_blockInfo.Offset < sizeOfHeader)
                throw new PackageBlockOffsetErrorException(_blockInfo.Offset, sizeOfHeader);

            var blockSize = (_blockInfo.Flags & BlockFlags.BlockCompressed) > 0
                ? _blockInfo.CompressedSize
                : _blockInfo.OriginalSize;

            if (inputStream.Length < _blockInfo.Offset + blockSize)
                throw new PackageStreamDataErrorException(_blockInfo.Offset + blockSize, inputStream.Length);
        }
    }
}