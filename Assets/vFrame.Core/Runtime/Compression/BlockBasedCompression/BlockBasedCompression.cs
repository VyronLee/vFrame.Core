// ------------------------------------------------------------
//         File: BlockBasedCompression.cs
//        Brief: Block-based compression and decompression with a structured file format
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 22:55:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.IO;
using System.Linq;
using System.Text;
// Compatibility-only dependency: Profiles remains available for legacy metadata/config paths,
// but it is no longer a retained modernization investment area.
using ByteArrayPool = System.Buffers.ArrayPool<byte>;

namespace vFrame.Core
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

        /// <summary>
        ///     Returns the serialized size of the header in bytes.
        /// </summary>
        /// <returns>The header size in bytes.</returns>
        public static int GetStructSize() {
            return sizeof(long) * 3 + sizeof(int) * 3 + 32;
        }
    }

    public class BlockBasedCompressionBlockTable
    {
        private int _blockIterator;
        public BlockBasedCompressionBlockInfo[] BlockInfos;

        /// <summary>
        ///     Finds the block info for the specified block index.
        /// </summary>
        /// <param name="blockIndex">Zero-based block index to look up.</param>
        /// <returns>The matching block info, or <c>null</c> if not found.</returns>
        /// <exception cref="BlockIndexOutOfRangeException">Thrown when <paramref name="blockIndex" /> is out of range.</exception>
        public BlockBasedCompressionBlockInfo FindBlock(int blockIndex) {
            if (null == BlockInfos) {
                return null;
            }

            if (blockIndex < 0 || blockIndex >= BlockInfos.Length) {
                ThrowHelper.ThrowIndexOutOfRangeException(0, BlockInfos.Length, blockIndex);
            }

            foreach (var blockInfo in BlockInfos) {
                if (blockInfo.BlockIndex == blockIndex) {
                    return blockInfo;
                }
            }

            return null;
        }

        /// <summary>
        ///     Adds a block info entry at the next available slot in the block table.
        /// </summary>
        /// <param name="blockInfo">The block info to add.</param>
        /// <exception cref="BlockIndexOutOfRangeException">Thrown when the internal iterator is out of range.</exception>
        public void AddBlock(BlockBasedCompressionBlockInfo blockInfo) {
            if (null == BlockInfos) {
                return;
            }

            if (_blockIterator < 0 || _blockIterator >= BlockInfos.Length) {
                throw new BlockIndexOutOfRangeException();
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
    ///     Block-based compression with a structured file format:
    ///     header, block data, and block table.
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

        /// <summary>
        ///     Initializes the byte array pool used for block compression buffers.
        /// </summary>
        protected override void OnCreate() {
            _buffers = ByteArrayPool.Create();
        }

        /// <summary>
        ///     Releases the byte array pool reference.
        /// </summary>
        protected override void OnDestroy() {
            _buffers = null;
        }

        /// <summary>
        ///     Reads and validates the compression header from the input stream.
        /// </summary>
        /// <param name="input">The input stream positioned at the header.</param>
        /// <returns>The parsed header.</returns>
        /// <exception cref="InvalidBlockBasedCompressionFormatException">Thrown when the header is malformed or validation fails.</exception>
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

        /// <summary>
        ///     Writes the block table to the output stream at the offset recorded in the header.
        /// </summary>
        /// <param name="output">The output stream to write to.</param>
        /// <param name="blockTable">The block table containing all block metadata.</param>
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

        /// <summary>
        ///     Initializes the compression session by computing the header, block count, and
        ///     positioning the output stream past the header area.
        /// </summary>
        /// <param name="input">The input data stream.</param>
        /// <param name="output">The output stream for compressed data.</param>
        /// <param name="options">Compression configuration options.</param>
        protected void BeginCompress(Stream input, Stream output, BlockBasedCompressionOptions options) {
            lock (_inputLock) {
                _inputStart = input.Position;
                _header = CreateHeader(input, options);
                _blockTable = CreateBlockTable(_header);
            }

            lock (_outputLock) {
                _outputStart = output.Position;
                output.Seek(_outputStart + BlockBasedCompressionHeader.GetStructSize(), SeekOrigin.Begin);
            }
        }

        /// <summary>
        ///     Finalizes the compression session by writing the header and block table.
        /// </summary>
        /// <param name="output">The output stream containing compressed data.</param>
        protected void EndCompress(Stream output) {
            _header.BlockTableOffset = BlockBasedCompressionHeader.GetStructSize();

            lock (_blockTable) {
                var lastBlock = _blockTable?.BlockInfos?.Last();
                if (null != lastBlock) {
                    _header.BlockTableOffset = lastBlock.BlockOffset + lastBlock.CompressedSize;
                }

                WriteHeader(output, _header);
                WriteBlockTable(output, _blockTable);
            }
        }

        /// <summary>
        ///     Calculates the number of blocks needed to cover the remaining input data.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="blockSize">The size of each block in bytes.</param>
        /// <returns>The total number of blocks.</returns>
        private static int CalculateBlockCount(Stream input, int blockSize) {
            return (int)Math.Ceiling((input.Length - input.Position) / (double)blockSize);
        }

        /// <summary>
        ///     Validates that the header contains expected magic ID, version, compressor type, and MD5.
        /// </summary>
        /// <param name="header">The header to validate.</param>
        /// <returns><c>true</c> if the header is valid; otherwise <c>false</c>.</returns>
        private static bool ValidateHeader(BlockBasedCompressionHeader header) {
            var ret = true;
            ret &= header.Id == BlockBasedCompressionConst.Id;
            ret &= header.Version == BlockBasedCompressionConst.Version;
            ret &= header.CompressorType != CompressorType.Invalid;
            ret &= header.Md5 != string.Empty;
            return ret;
        }

        /// <summary>
        ///     Creates a compression header from the input stream and options.
        /// </summary>
        /// <param name="input">The input stream to compute MD5 and block count from.</param>
        /// <param name="options">Compression configuration options.</param>
        /// <returns>A fully initialized compression header.</returns>
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

        /// <summary>
        ///     Creates an empty block table pre-allocated for the given header's block count.
        /// </summary>
        /// <param name="header">The compression header containing the block count.</param>
        /// <returns>A new block table with pre-allocated block info slots.</returns>
        private static BlockBasedCompressionBlockTable CreateBlockTable(BlockBasedCompressionHeader header) {
            var blockTable = new BlockBasedCompressionBlockTable {
                BlockInfos = new BlockBasedCompressionBlockInfo[header.BlockCount]
            };
            return blockTable;
        }

        /// <summary>
        ///     Writes the compression header to the beginning of the output stream.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="header">The header to write.</param>
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

        /// <summary>
        ///     Compresses a single block from the input stream and appends it to the output stream.
        ///     Thread-safe: acquires input and output locks internally.
        /// </summary>
        /// <param name="input">The input data stream.</param>
        /// <param name="output">The output stream for compressed data.</param>
        /// <param name="options">Compression configuration options.</param>
        /// <param name="blockIndex">Zero-based index of the block to compress.</param>
        protected void SafeCompress(Stream input,
            Stream output,
            BlockBasedCompressionOptions options,
            int blockIndex) {
            var dataBuffer = _buffers.Rent(options.BlockSize);
            var outBuffer = _buffers.Rent(options.BlockSize);

            SafeReadRawBlockData(input, options, blockIndex, ref dataBuffer, out var dataLength);
            SafeBufferedCompress(dataBuffer, dataLength, options, ref outBuffer, out var outLength, out var compressed);
            SafeWriteCompressedDataToOutput(output, outBuffer, outLength, out var offset);
            SafeSaveBlockInfo(blockIndex, offset, dataLength, outLength, compressed);

            _buffers.Return(dataBuffer);
            _buffers.Return(outBuffer);
        }

        /// <summary>
        ///     Reads a raw block of data from the input stream under the input lock.
        /// </summary>
        /// <param name="input">The input data stream.</param>
        /// <param name="options">Compression options containing block size.</param>
        /// <param name="blockIndex">Zero-based block index to read.</param>
        /// <param name="dataBuffer">Rented buffer to receive the raw block data.</param>
        /// <param name="dataLength">The number of bytes actually read.</param>
        /// <exception cref="DataNotEnoughException">Thrown when the block offset exceeds the stream length.</exception>
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

                dataLength = input.Read(dataBuffer, 0, options.BlockSize);
            }
        }

        /// <summary>
        ///     Compresses a block of data in memory. If the compressed output is larger than the input,
        ///     the original uncompressed data is used instead.
        /// </summary>
        /// <param name="dataBuffer">Buffer containing the raw data.</param>
        /// <param name="dataLength">Length of the raw data in bytes.</param>
        /// <param name="options">Compression options specifying the compressor and settings.</param>
        /// <param name="outBuffer">Rented buffer to receive the output data.</param>
        /// <param name="outLength">The number of bytes written to <paramref name="outBuffer" />.</param>
        /// <param name="compressed"><c>true</c> if the data was compressed; <c>false</c> if stored raw.</param>
        private void SafeBufferedCompress(byte[] dataBuffer,
            int dataLength,
            BlockBasedCompressionOptions options,
            ref byte[] outBuffer,
            out int outLength,
            out bool compressed) {
            using (var compressor = CompressorPool.Instance().Rent(options.CompressorType, options.CompressOptions)) {
                using (var inStream = new MemoryStream(dataBuffer, 0, dataLength)) {
                    using (var outStream = new MemoryStream(dataLength)) {
                        outStream.SetLength(0);
                        compressor.Compress(inStream, outStream);

                        if (outStream.Length > outBuffer.Length) {
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

        /// <summary>
        ///     Writes compressed block data to the output stream under the output lock.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="dataBuffer">Buffer containing the data to write.</param>
        /// <param name="dataLength">Number of bytes to write from <paramref name="dataBuffer" />.</param>
        /// <param name="offset">The output stream position where the data was written.</param>
        private void SafeWriteCompressedDataToOutput(Stream output, byte[] dataBuffer, int dataLength,
            out long offset) {
            lock (_outputLock) {
                offset = output.Position;
                output.Write(dataBuffer, 0, dataLength);
            }
        }

        /// <summary>
        ///     Records block metadata into the block table under the block table lock.
        /// </summary>
        /// <param name="blockIndex">Zero-based block index.</param>
        /// <param name="offset">Offset of the block data in the output stream.</param>
        /// <param name="originSize">Original uncompressed size of the block.</param>
        /// <param name="compressedSize">Compressed size of the block.</param>
        /// <param name="compressed">Whether the block was actually compressed.</param>
        /// <exception cref="BlockIndexOutOfRangeException">Thrown when <paramref name="blockIndex" /> is out of range.</exception>
        private void SafeSaveBlockInfo(int blockIndex, long offset, int originSize, int compressedSize,
            bool compressed) {
            lock (_blockTable) {
                if (blockIndex < 0 || blockIndex >= _blockTable.BlockInfos.Length) {
                    throw new BlockIndexOutOfRangeException();
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

        /// <summary>
        ///     Initializes the decompression session by reading the header and block table
        ///     from the input stream, and positioning the output stream.
        /// </summary>
        /// <param name="input">The compressed input stream.</param>
        /// <param name="output">The output stream for decompressed data.</param>
        protected void BeginDecompress(Stream input, Stream output) {
            lock (_inputLock) {
                _inputStart = input.Position;
                _header = ReadHeader(input);
                _blockTable = ReadBlockTable(input);
            }

            lock (_outputLock) {
                _outputStart = output.Position;
                output.Seek(_outputStart, SeekOrigin.Begin);
            }
        }

        /// <summary>
        ///     Finalizes the decompression session by optionally validating the output MD5 hash.
        /// </summary>
        /// <param name="output">The output stream containing decompressed data.</param>
        /// <exception cref="HashNotMatchException">
        ///     Thrown when MD5 validation fails and <see cref="SkipValidation" /> is
        ///     <c>false</c>.
        /// </exception>
        protected void EndDecompress(Stream output) {
            lock (_outputLock) {
                output.Seek(_outputStart, SeekOrigin.Begin);

                if (!SkipValidation) {
                    var md5 = MessageDigestUtils.MD5(output);
                    if (md5 != _header.Md5) {
                        throw new HashNotMatchException();
                    }
                }
            }
        }

        /// <summary>
        ///     Reads the block table from the input stream at the offset specified in the header.
        /// </summary>
        /// <param name="input">The compressed input stream.</param>
        /// <returns>The populated block table.</returns>
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

        /// <summary>
        ///     Decompresses a single block from the input stream and writes it to the output stream.
        ///     Thread-safe: acquires input and output locks internally.
        /// </summary>
        /// <param name="input">The compressed input stream.</param>
        /// <param name="output">The output stream for decompressed data.</param>
        /// <param name="blockIndex">Zero-based index of the block to decompress.</param>
        protected void SafeDecompress(Stream input, Stream output, int blockIndex) {
            var dataBuffer = _buffers.Rent(_header.BlockSize);
            var outBuffer = _buffers.Rent(_header.BlockSize);

            SafeReadCompressedBlockData(input, blockIndex, ref dataBuffer, out var dataLength, out var compressed);
            SafeBufferedDecompress(dataBuffer, dataLength, compressed, ref outBuffer, out var outLength);
            SafeWriteRawDataToOutput(output, outBuffer, outLength, blockIndex);

            _buffers.Return(dataBuffer);
            _buffers.Return(outBuffer);
        }

        /// <summary>
        ///     Reads a compressed block from the input stream under the input lock.
        /// </summary>
        /// <param name="input">The compressed input stream.</param>
        /// <param name="blockIndex">Zero-based block index to read.</param>
        /// <param name="dataBuffer">Rented buffer to receive the compressed data.</param>
        /// <param name="dataLength">The number of bytes read into <paramref name="dataBuffer" />.</param>
        /// <param name="compressed"><c>true</c> if the block was stored compressed.</param>
        /// <exception cref="BlockTableDataErrorException">Thrown when the block info is not found in the table.</exception>
        /// <exception cref="DataNotEnoughException">Thrown when the stream does not contain enough data.</exception>
        /// <exception cref="BufferSizeTooSmallException">Thrown when the rented buffer is too small for the block data.</exception>
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

                if (input.Position + blockInfo.CompressedSize > input.Length) {
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

        /// <summary>
        ///     Writes decompressed raw data to the output stream at the correct block position.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="dataBuffer">Buffer containing the decompressed data.</param>
        /// <param name="dataLength">Number of bytes to write.</param>
        /// <param name="blockIndex">Zero-based block index used to compute the output position.</param>
        private void SafeWriteRawDataToOutput(Stream output, byte[] dataBuffer, int dataLength, int blockIndex) {
            lock (_outputLock) {
                output.Seek(_outputStart + blockIndex * (long)_header.BlockSize, SeekOrigin.Begin);
                output.Write(dataBuffer, 0, dataLength);
            }
        }

        /// <summary>
        ///     Decompresses a block of data in memory. If the block was not compressed, the data is copied as-is.
        /// </summary>
        /// <param name="dataBuffer">Buffer containing the block data.</param>
        /// <param name="dataLength">Length of the data in bytes.</param>
        /// <param name="compressed">Whether the data was compressed.</param>
        /// <param name="outBuffer">Rented buffer to receive the decompressed output.</param>
        /// <param name="outLength">The number of bytes written to <paramref name="outBuffer" />.</param>
        /// <exception cref="BufferSizeTooLargeException">Thrown when decompressed output exceeds <see cref="int.MaxValue" />.</exception>
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