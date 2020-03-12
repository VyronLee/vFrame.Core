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
using vFrame.Core.Compress;
using vFrame.Core.Crypto;
using vFrame.Core.FileSystems.Constants;
using vFrame.Core.FileSystems.Exceptions;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.FileSystems.Package
{
    public class PackageStream : Stream
    {
        private readonly PackageBlockInfo _blockInfo;
        private readonly FileAccess _mode = FileAccess.ReadWrite;

        private readonly Stream _pkgStream;
        private MemoryStream _memoryStream;
        private bool _opened;
        private bool _closed;

        internal PackageStream(Stream pkgStream, PackageBlockInfo blockInfo) {
            _pkgStream = pkgStream;
            _blockInfo = blockInfo;
        }

        internal PackageStream(Stream pkgStream, PackageBlockInfo blockInfo, FileAccess mode)
            : this(pkgStream, blockInfo) {
            _mode = mode;
        }

        public bool Open() {
            Debug.Assert(null != _pkgStream);
            return InternalOpen(_pkgStream);
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

        public override bool CanRead => _opened && !_closed && (_mode & FileAccess.Read) > 0;
        public override bool CanSeek => _opened && !_closed;
        public override bool CanWrite => _opened && !_closed && (_mode & FileAccess.Write) > 0;
        public override long Length => _blockInfo.OriginalSize;

        public override long Position {
            get => _memoryStream.Position;
            set => _memoryStream.Position = value;
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

            _memoryStream = new MemoryStream();
            _memoryStream.SetLength(_blockInfo.OriginalSize);

            inputStream.Seek(_blockInfo.Offset, SeekOrigin.Begin);

            using (var tempStream = new MemoryStream()) {
                inputStream.CopyTo(tempStream);

                if ((_blockInfo.Flags & BlockFlags.BlockCompressed) > 0) {
                    var compressService = CompressService.CreateCompressService((CompressType) (_blockInfo.Flags >> 8));
                    compressService.Decompress(inputStream, tempStream);
                    compressService.Destroy();
                    ObjectPoolManager.Instance().Return(compressService);
                }

                if ((_blockInfo.Flags & BlockFlags.BlockEncrypted) > 0) {
                    var keyBytes = BitConverter.GetBytes(_blockInfo.EncryptKey);
                    var cryptoService = CryptoService.CreateCrypto((CryptoType) (_blockInfo.Flags >> 12));
                    cryptoService.Decrypt(tempStream, _memoryStream, keyBytes, keyBytes.Length);
                    cryptoService.Destroy();
                    ObjectPoolManager.Instance().Return(cryptoService);
                }
                else {
                    tempStream.CopyTo(_memoryStream);
                }
            }

            _memoryStream.Seek(0, SeekOrigin.Begin);

            return _opened = true;
        }

        private void ValidateBlockInfo(Stream inputStream) {
            if ((_blockInfo.Flags & BlockFlags.BlockExists) <= 0) throw new PackageBlockDisposedException();

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