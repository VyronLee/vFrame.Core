﻿using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using vFrame.Core.Base;
using vFrame.Core.Extensions;
using vFrame.Core.Profiles;
using vFrame.Core.Utils;
using ByteArrayPool = System.Buffers.ArrayPool<byte>;

namespace vFrame.Core.Compression
{
    public class BlockBasedCompressionHeader
    {
        public long Id { get; set; }
        public long Version { get; set; }
        public CompressorType CompressorType { get; set; } = CompressorType.LZMA;
        public int BlockSize { get; set; } = 1024;
        public int BlockCount { get; set; }
        public long BlockTableOffset { get; set; }
        public string Md5 { get; set; }

        public static int GetStructSize() {
            return sizeof(long) * 3 + sizeof(int) * 3 + 32;
        }
    }

    public class BlockBasedCompressionBlockTable
    {
        private int _blockIterator;
        public BlockBasedCompressionBlockInfo[] BlockInfos;

        public BlockBasedCompressionBlockInfo FindBlock(int blockIndex) {
            if (null == BlockInfos) {
                return null;
            }

            if (blockIndex < 0 || blockIndex >= BlockInfos.Length) {
                throw new IndexOutOfRangeException();
            }

            foreach (var blockInfo in BlockInfos) {
                if (blockInfo.BlockIndex == blockIndex) {
                    return blockInfo;
                }
            }
            return null;
        }

        public void AddBlock(BlockBasedCompressionBlockInfo blockInfo) {
            if (null == BlockInfos) {
                return;
            }
            if (_blockIterator < 0 || _blockIterator >= BlockInfos.Length) {
                throw new IndexOutOfRangeException();
            }
            BlockInfos[_blockIterator++] = blockInfo;
        }
    }

    public class BlockBasedCompressionBlockInfo
    {
        public int BlockIndex;
        public long BlockOffset;
        public bool Compressed;
        public int CompressedSize;
        public int OriginSize;
    }

    /// <summary>
    ///     Block Based Compression
    ///     File format:
    ///     |- header
    ///     |- block data 1
    ///     |- block data 2
    ///     |- block data 3
    ///     |- ...
    ///     |- block table
    /// </summary>
    public class BlockBasedCompression : BaseObject
    {
        private readonly object _inputLock = new object();
        private readonly object _outputLock = new object();
        private BlockBasedCompressionBlockTable _blockTable;

        private ByteArrayPool _buffers;

        private BlockBasedCompressionHeader _header;
        private long _inputStart;
        private long _outputStart;

        protected int BlockCount => _header?.BlockCount ?? 0;

        public bool SkipValidation { get; set; } = false;

        protected override void OnCreate() {
            _buffers = ByteArrayPool.Create();
        }

        protected override void OnDestroy() {
            _buffers = null;
        }

        private BlockBasedCompressionHeader ReadHeader(Stream input) {
            var header = new BlockBasedCompressionHeader();
            try {
                lock (_inputLock) {
                    using (var reader = new BinaryReader(input, Encoding.UTF8, true)) {
                        header.Id = reader.ReadInt64();
                        header.Version = reader.ReadInt64();
                        header.CompressorType = (CompressorType)reader.ReadInt32();
                        header.BlockSize = reader.ReadInt32();
                        header.BlockCount = reader.ReadInt32();
                        header.BlockTableOffset = reader.ReadInt64();
                        header.Md5 = reader.ReadBytes(32).ToStr();
                    }
                }
            }
            catch (Exception) {
                throw new InvalidBlockBasedCompressionFormatException();
            }

            if (!ValidateHeader(header)) {
                throw new InvalidBlockBasedCompressionFormatException();
            }
            return header;
        }

        private void WriteBlockTable(Stream output, BlockBasedCompressionBlockTable blockTable) {
            lock (_outputLock) {
                output.Seek(_header.BlockTableOffset, SeekOrigin.Begin);
                using (var writer = new BinaryWriter(output, Encoding.UTF8, true)) {
                    foreach (var blockInfo in blockTable.BlockInfos) {
                        writer.Write(blockInfo.BlockIndex);
                        writer.Write(blockInfo.BlockOffset);
                        writer.Write(blockInfo.OriginSize);
                        writer.Write(blockInfo.CompressedSize);
                        writer.Write(blockInfo.Compressed);
                    }
                }
            }
        }

        #region Compress

        protected void BeginCompress(Stream input, Stream output, BlockBasedCompressionOptions options) {
            PerfProfile.Start(out var id);
            PerfProfile.Pin("BeginCompress", id);
            lock (_inputLock) {
                _inputStart = input.Position;
                _header = CreateHeader(input, options);
                _blockTable = CreateBlockTable(_header);
            }

            lock (_outputLock) {
                _outputStart = output.Position;
                output.Seek(_outputStart + BlockBasedCompressionHeader.GetStructSize(), SeekOrigin.Begin);
            }
            PerfProfile.Unpin(id);
        }

        protected void EndCompress(Stream output) {
            PerfProfile.Start(out var id);
            PerfProfile.Pin("EndCompress", id);
            _header.BlockTableOffset = BlockBasedCompressionHeader.GetStructSize();

            lock (_blockTable) {
                var lastBlock = _blockTable?.BlockInfos?.Last();
                if (null != lastBlock) {
                    _header.BlockTableOffset = lastBlock.BlockOffset + lastBlock.CompressedSize;
                }

                WriteHeader(output, _header);
                WriteBlockTable(output, _blockTable);
            }
            PerfProfile.Unpin(id);
        }

        private static int CalculateBlockCount(Stream input, int blockSize) {
            return (int)Math.Ceiling((input.Length - input.Position) / (double)blockSize);
        }

        private static bool ValidateHeader(BlockBasedCompressionHeader header) {
            var ret = true;
            ret &= header.Id == BlockBasedCompressionConst.Id;
            ret &= header.Version == BlockBasedCompressionConst.Version;
            ret &= header.CompressorType != CompressorType.Invalid;
            ret &= header.Md5 != string.Empty;
            return ret;
        }

        private static BlockBasedCompressionHeader CreateHeader(Stream input, BlockBasedCompressionOptions options) {
            var blockCount = CalculateBlockCount(input, options.BlockSize);
            var md5 = MessageDigestUtils.MD5(input);
            var header = new BlockBasedCompressionHeader {
                Id = BlockBasedCompressionConst.Id,
                Version = BlockBasedCompressionConst.Version,
                CompressorType = options.CompressorType,
                BlockSize = options.BlockSize,
                BlockCount = blockCount,
                Md5 = md5
            };
            return header;
        }

        private static BlockBasedCompressionBlockTable CreateBlockTable(BlockBasedCompressionHeader header) {
            var blockTable = new BlockBasedCompressionBlockTable {
                BlockInfos = new BlockBasedCompressionBlockInfo[header.BlockCount]
            };
            return blockTable;
        }

        private void WriteHeader(Stream output, BlockBasedCompressionHeader header) {
            lock (_outputLock) {
                output.Seek(_outputStart, SeekOrigin.Begin);
                using (var writer = new BinaryWriter(output, Encoding.UTF8, true)) {
                    writer.Write(BlockBasedCompressionConst.Id);
                    writer.Write(BlockBasedCompressionConst.Version);
                    writer.Write((int)header.CompressorType);
                    writer.Write(header.BlockSize);
                    writer.Write(header.BlockCount);
                    writer.Write(header.BlockTableOffset);
                    writer.Write(header.Md5.ToByteArray());
                }
            }
        }

        protected void SafeCompress(Stream input,
            Stream output,
            BlockBasedCompressionOptions options,
            int blockIndex) {
            PerfProfile.Start(out var id);
            PerfProfile.Pin("SafeCompress, blockIndex: " + blockIndex, id);

            var dataBuffer = _buffers.Rent(options.BlockSize);
            var outBuffer = _buffers.Rent(options.BlockSize);

            SafeReadRawBlockData(input, options, blockIndex, ref dataBuffer, out var dataLength);
            SafeBufferedCompress(dataBuffer, dataLength, options, ref outBuffer, out var outLength, out var compressed);
            SafeWriteCompressedDataToOutput(output, outBuffer, outLength, out var offset);
            SafeSaveBlockInfo(blockIndex, offset, dataLength, outLength, compressed);

            _buffers.Return(dataBuffer);
            _buffers.Return(outBuffer);

            PerfProfile.Unpin(id);
        }

        private void SafeReadRawBlockData(Stream input,
            BlockBasedCompressionOptions options,
            int blockIndex,
            ref byte[] dataBuffer,
            out int dataLength) {
            lock (_inputLock) {
                var blockOffset = _inputStart + blockIndex * (long)options.BlockSize;
                input.Seek(blockOffset, SeekOrigin.Begin);

                if (blockOffset >= input.Length) {
                    throw new DataNotEnoughException();
                }

                // Read data
                dataLength = input.Read(dataBuffer, 0, options.BlockSize);
            }
        }

        private void SafeBufferedCompress(byte[] dataBuffer,
            int dataLength,
            BlockBasedCompressionOptions options,
            ref byte[] outBuffer,
            out int outLength,
            out bool compressed) {
            using (var compressor = CompressorPool.Instance().Rent(options.CompressorType, options.CompressOptions)) {
                using (var inStream = new MemoryStream(dataBuffer, 0, dataLength)) {
                    //using (var outStream = new MemoryStream(outBuffer)) {  //System.NotSupportedException: Memory stream is not expandable.
                    using (var outStream = new MemoryStream(dataLength)) {
                        outStream.SetLength(0);
                        compressor.Compress(inStream, outStream);

                        if (outStream.Length > outBuffer.Length) { // Discard if size is larger after compressed.
                            Array.Copy(dataBuffer, 0, outBuffer, 0, dataLength);
                            outLength = dataLength;
                            compressed = false;
                        }
                        else {
                            Array.Copy(outStream.GetBuffer(), 0, outBuffer, 0, outStream.Length);
                            outLength = (int)outStream.Length;
                            compressed = true;
                        }
                    }
                }
            }
        }

        private void SafeWriteCompressedDataToOutput(Stream output, byte[] dataBuffer, int dataLength,
            out long offset) {
            lock (_outputLock) {
                offset = output.Position;
                output.Write(dataBuffer, 0, dataLength);
            }
        }

        private void SafeSaveBlockInfo(int blockIndex, long offset, int originSize, int compressedSize,
            bool compressed) {
            lock (_blockTable) {
                if (blockIndex < 0 || blockIndex > _blockTable.BlockInfos.Length) {
                    throw new IndexOutOfRangeException();
                }

                var blockInfo = new BlockBasedCompressionBlockInfo {
                    BlockIndex = blockIndex,
                    BlockOffset = offset,
                    OriginSize = originSize,
                    CompressedSize = compressedSize,
                    Compressed = compressed
                };
                _blockTable.AddBlock(blockInfo);
            }
        }

        #endregion


        #region Decompress

        protected void BeginDecompress(Stream input, Stream output) {
            PerfProfile.Start(out var id);
            PerfProfile.Pin("BeginDecompress", id);

            lock (_inputLock) {
                _inputStart = input.Position;
                _header = ReadHeader(input);
                _blockTable = ReadBlockTable(input);
            }

            lock (_outputLock) {
                _outputStart = output.Position;
                output.Seek(_outputStart, SeekOrigin.Begin);
            }

            PerfProfile.Unpin(id);
        }

        protected void EndDecompress(Stream output) {
            PerfProfile.Start(out var id);
            PerfProfile.Pin("EndDecompress", id);

            lock (_outputLock) {
                output.Seek(_outputStart, SeekOrigin.Begin);

                if (!SkipValidation) {
                    var md5 = MessageDigestUtils.MD5(output);
                    if (md5 != _header.Md5) {
                        throw new HashNotMatchException();
                    }
                }
            }

            PerfProfile.Unpin(id);
        }

        private BlockBasedCompressionBlockTable ReadBlockTable(Stream input) {
            var blockTable = new BlockBasedCompressionBlockTable {
                BlockInfos = new BlockBasedCompressionBlockInfo[_header.BlockCount]
            };

            lock (_inputLock) {
                input.Seek(_header.BlockTableOffset, SeekOrigin.Begin);
                using (var reader = new BinaryReader(input, Encoding.UTF8, true)) {
                    for (var i = 0; i < _header.BlockCount; i++) {
                        blockTable.BlockInfos[i] = new BlockBasedCompressionBlockInfo {
                            BlockIndex = reader.ReadInt32(),
                            BlockOffset = reader.ReadInt64(),
                            OriginSize = reader.ReadInt32(),
                            CompressedSize = reader.ReadInt32(),
                            Compressed = reader.ReadBoolean()
                        };
                    }
                }
            }

            return blockTable;
        }

        protected void SafeDecompress(Stream input, Stream output, int blockIndex) {
            PerfProfile.Start(out var id);
            PerfProfile.Pin("SafeDecompress, blockIndex: " + blockIndex, id);

            var dataBuffer = _buffers.Rent(_header.BlockSize);
            var outBuffer = _buffers.Rent(_header.BlockSize);

            SafeReadCompressedBlockData(input, blockIndex, ref dataBuffer, out var dataLength, out var compressed);
            SafeBufferedDecompress(dataBuffer, dataLength, compressed, ref outBuffer, out var outLength);
            SafeWriteRawDataToOutput(output, outBuffer, outLength, blockIndex);

            _buffers.Return(dataBuffer);
            _buffers.Return(outBuffer);

            PerfProfile.Unpin(id);
        }

        private void SafeReadCompressedBlockData(Stream input,
            int blockIndex,
            ref byte[] dataBuffer,
            out int dataLength,
            out bool compressed) {
            var blockInfo = _blockTable.FindBlock(blockIndex);
            if (null == blockInfo) {
                throw new BlockTableDataErrorException();
            }

            lock (_inputLock) {
                input.Seek(blockInfo.BlockOffset, SeekOrigin.Begin);

                // Read data
                if (input.Position + blockInfo.CompressedSize >= input.Length) {
                    throw new DataNotEnoughException();
                }

                if (dataBuffer.Length < blockInfo.CompressedSize) {
                    throw new BufferSizeTooSmallException(dataBuffer, blockInfo.CompressedSize);
                }

                var lengthRead = input.Read(dataBuffer, 0, blockInfo.CompressedSize);
                if (lengthRead != blockInfo.CompressedSize) {
                    throw new DataNotEnoughException();
                }
                dataLength = lengthRead;
            }
            compressed = blockInfo.Compressed;
        }

        private void SafeWriteRawDataToOutput(Stream output, byte[] dataBuffer, int dataLength, int blockIndex) {
            lock (_outputLock) {
                output.Seek(_outputStart + blockIndex * (long)_header.BlockSize, SeekOrigin.Begin);
                output.Write(dataBuffer, 0, dataLength);
            }
        }

        private void SafeBufferedDecompress(byte[] dataBuffer,
            int dataLength,
            bool compressed,
            ref byte[] outBuffer,
            out int outLength) {
            if (!compressed) {
                Array.Copy(dataBuffer, 0, outBuffer, 0, outLength = dataLength);
                return;
            }

            using (var compressor = CompressorPool.Instance().Rent(_header.CompressorType)) {
                using (var inStream = new MemoryStream(dataBuffer, 0, dataLength)) {
                    using (var outStream = new MemoryStream(outBuffer)) {
                        outStream.SetLength(0);
                        compressor.Decompress(inStream, outStream);
                        if (outStream.Length > int.MaxValue) {
                            throw new BufferSizeTooLargeException(outStream.Length);
                        }
                        outLength = (int)outStream.Length;
                    }
                }
            }
        }

        #endregion
    }
}