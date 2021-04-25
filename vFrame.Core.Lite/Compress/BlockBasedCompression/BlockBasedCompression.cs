using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using vFrame.Core.Base;
using vFrame.Core.Compress.Services;
using vFrame.Core.Extensions;
using vFrame.Core.FileSystems.Exceptions;
using vFrame.Core.Utils;

namespace vFrame.Core.Compress.BlockBasedCompression
{
    public class BlockBasedCompressionHeader
    {
        public long Id;
        public long Version;
        public CompressType CompressType { get; set; } = CompressType.LZMA;
        public int BlockSize { get; set; } = 1024;
        public int BlockCount { get; set; }
        public int BlockTableOffset { get; set; }
        public string Md5 { get; set; }

        public static int GetMarshalSize() {
            return sizeof(long) * 2 + sizeof(int) * 4 + 32;
        }
    }

    public class BlockBasedCompressionBlockTable
    {
        public BlockBasedCompressionBlockInfo[] BlockInfos;

        private int _blockIterator;

        public BlockBasedCompressionBlockInfo FindBlock(int blockIndex) {
            if (null == BlockInfos) {
                return null;
            }

            if (blockIndex < 0 || blockIndex >= BlockInfos.Length) {
                throw new BlockBasedCompressionIndexOutOfRangeException();
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
                throw new BlockBasedCompressionIndexOutOfRangeException();
            }
            BlockInfos[_blockIterator++] = blockInfo;
        }
    }

    public class BlockBasedCompressionBlockInfo
    {
        public int BlockIndex;
        public int BlockOffset;
        public int OriginSize;
        public int CompressedSize;
    }

    /// <summary>
    /// Block Based Compression
    ///     File format:
    ///         |- header
    ///         |- block data 1
    ///         |- block data 2
    ///         |- block data 3
    ///         |- ...
    ///         |- block table
    /// </summary>
    public class BlockBasedCompression : BaseObject
    {
        private readonly object _inputLock = new object();
        private readonly object _outputLock = new object();

        private ArrayPool<byte> _buffers;

        private BlockBasedCompressionHeader _header;
        private BlockBasedCompressionBlockTable _blockTable;
        private int _inputStart;
        private int _outputStart;

        protected int BlockCount => _header?.BlockCount ?? 0;

        protected override void OnCreate() {
            _buffers = ArrayPool<byte>.Create();
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
                        header.CompressType = (CompressType) reader.ReadInt32();
                        header.BlockSize = reader.ReadInt32();
                        header.BlockCount = reader.ReadInt32();
                        header.BlockTableOffset = reader.ReadInt32();
                        header.Md5 = reader.ReadBytes(32).ToStr();
                        return header;
                    }
                }
            }
            catch (Exception) {
                throw new InvalidBlockBasedCompressionFormatException();
            }
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
                    }
                }
            }
        }

        #region Compress

        protected void BeginCompress(Stream input, Stream output, BlockBasedCompressionOptions options) {
            lock (_inputLock) {
                _inputStart = (int) input.Position;
                _header = CreateHeader(input, options);
                _blockTable = CreateBlockTable(_header);
            }

            lock (_outputLock) {
                _outputStart = (int) output.Position;
                output.Seek(_outputStart + BlockBasedCompressionHeader.GetMarshalSize(), SeekOrigin.Begin);
            }
        }

        protected void EndCompress(Stream output) {
            _header.BlockTableOffset = BlockBasedCompressionHeader.GetMarshalSize();

            lock (_blockTable) {
                var lastBlock = _blockTable?.BlockInfos?.Last();
                if (null != lastBlock) {
                    _header.BlockTableOffset = lastBlock.BlockOffset + lastBlock.CompressedSize;
                }

                WriteHeader(output, _header);
                WriteBlockTable(output, _blockTable);
            }
        }

        private static int CalculateBlockCount(Stream input, int blockSize) {
            return (int)Math.Ceiling((input.Length - input.Position) / (double)blockSize);
        }

        private static BlockBasedCompressionHeader CreateHeader(Stream input, BlockBasedCompressionOptions options) {
            var blockCount = CalculateBlockCount(input, options.BlockSize);
            var md5 = MessageDigestUtils.MD5(input);
            var header = new BlockBasedCompressionHeader {
                Id = BlockBasedCompressionConst.Id,
                Version = BlockBasedCompressionConst.Version,
                CompressType = options.CompressType,
                BlockSize = options.BlockSize,
                BlockCount = blockCount,
                Md5 = md5
            };
            return header;
        }

        private static BlockBasedCompressionBlockTable CreateBlockTable(BlockBasedCompressionHeader header) {
            var blockTable = new BlockBasedCompressionBlockTable {
                BlockInfos = new BlockBasedCompressionBlockInfo[header.BlockCount],
            };
            return blockTable;
        }

        private void WriteHeader(Stream output, BlockBasedCompressionHeader header) {
            lock (_outputLock) {
                output.Seek(_outputStart, SeekOrigin.Begin);
                using (var writer = new BinaryWriter(output, Encoding.UTF8, true)) {
                    writer.Write(BlockBasedCompressionConst.Id);
                    writer.Write(BlockBasedCompressionConst.Version);
                    writer.Write((int)header.CompressType);
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
            int blockIndex)
        {
            var dataBuffer = _buffers.Rent(options.BlockSize);
            var outBuffer = _buffers.Rent(options.BlockSize);

            SafeReadRawBlockData(input, options, blockIndex, ref dataBuffer, out var dataLength);
            SafeBufferedCompress(dataBuffer, dataLength, options.CompressType, ref outBuffer, out var outLength);
            SafeWriteCompressedDataToOutput(output, outBuffer, outLength, out var offset);
            SafeSaveBlockInfo(blockIndex, offset, dataLength, outLength);

            _buffers.Return(dataBuffer);
            _buffers.Return(outBuffer);
        }

        private void SafeReadRawBlockData(Stream input,
            BlockBasedCompressionOptions options,
            int blockIndex,
            ref byte[] dataBuffer,
            out int dataLength)
        {
            lock (_inputLock) {
                var blockOffset = _inputStart + blockIndex * options.BlockSize;
                input.Seek(blockOffset, SeekOrigin.Begin);

                if (blockOffset >= input.Length) {
                    throw new BlockBasedCompressionDataNotEnoughException();
                }

                // Read data
                dataLength = input.Read(dataBuffer, 0, options.BlockSize);
            }
        }

        private void SafeBufferedCompress(byte[] dataBuffer,
            int dataLength,
            CompressType compressType,
            ref byte[] outBuffer,
            out int outLength)
        {
            var service = CompressService.CreateCompressService(compressType);
            using (var inStream = new MemoryStream(dataBuffer, 0, dataLength)) {
                using (var outStream = new MemoryStream(outBuffer)) {
                    outStream.SetLength(0);
                    service.Compress(inStream, outStream);
                    outLength = (int)outStream.Length;
                }
            }
            CompressService.DestroyCompressService(service);
        }

        private void SafeWriteCompressedDataToOutput(Stream output, byte[] dataBuffer, int dataLength, out int offset) {
            lock (_outputLock) {
                offset = (int)output.Position;
                output.Write(dataBuffer, 0, dataLength);
            }
        }

        private void SafeSaveBlockInfo(int blockIndex, int offset, int originSize, int compressedSize) {
            lock (_blockTable) {
                if (blockIndex < 0 || blockIndex > _blockTable.BlockInfos.Length) {
                    throw new BlockBasedCompressionIndexOutOfRangeException();
                }

                var blockInfo = new BlockBasedCompressionBlockInfo {
                    BlockIndex = blockIndex,
                    BlockOffset = offset,
                    OriginSize = originSize,
                    CompressedSize = compressedSize,
                };
                _blockTable.AddBlock(blockInfo);
            }
        }

        #endregion


        #region Decompress

        protected void BeginDecompress(Stream input, Stream output) {
            lock (_inputLock) {
                _inputStart = (int) input.Position;
                _header = ReadHeader(input);
                _blockTable = ReadBlockTable(input);
            }

            lock (_outputLock) {
                _outputStart = (int) output.Position;
                output.Seek(_outputStart, SeekOrigin.Begin);
            }
        }

        protected void EndDecompress(Stream output) {
            lock (_outputLock) {
                output.Seek(_outputStart, SeekOrigin.Begin);

                var md5 = MessageDigestUtils.MD5(output);
                if (md5 != _header.Md5) {
                    throw new BlockBasedCompressionHashNotMatchException();
                }
            }
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
                            BlockOffset = reader.ReadInt32(),
                            OriginSize = reader.ReadInt32(),
                            CompressedSize = reader.ReadInt32(),
                        };
                    }
                }
            }

            return blockTable;
        }

        protected void SafeDecompress(Stream input, Stream output, int blockIndex) {
            var dataBuffer = _buffers.Rent(_header.BlockSize);
            var outBuffer = _buffers.Rent(_header.BlockSize);

            SafeReadCompressedBlockData(input, blockIndex, ref dataBuffer, out var dataLength);
            SafeBufferedDecompress(dataBuffer, dataLength, ref outBuffer, out var outLength);
            SafeWriteRawDataToOutput(output, outBuffer, outLength, blockIndex);

            _buffers.Return(dataBuffer);
            _buffers.Return(outBuffer);
        }

        private void SafeReadCompressedBlockData(Stream input,
            int blockIndex,
            ref byte[] dataBuffer,
            out int dataLength)
        {
            var blockInfo = _blockTable.FindBlock(blockIndex);
            if (null == blockInfo) {
                throw new PackageFileSystemBlockTableDataError();
            }

            lock (_inputLock) {
                input.Seek(blockInfo.BlockOffset, SeekOrigin.Begin);

                // Read data
                if (input.Position + blockInfo.CompressedSize >= input.Length) {
                    throw new BlockBasedCompressionDataNotEnoughException();
                }

                var lengthRead = input.Read(dataBuffer, 0, blockInfo.CompressedSize);
                if (lengthRead != blockInfo.CompressedSize) {
                    throw new BlockBasedCompressionDataNotEnoughException();
                }
                dataLength = lengthRead;
            }
        }

        private void SafeWriteRawDataToOutput(Stream output, byte[] dataBuffer, int dataLength, int blockIndex) {
            lock (_outputLock) {
                output.Seek(_outputStart + blockIndex * _header.BlockSize, SeekOrigin.Begin);
                output.Write(dataBuffer, 0, dataLength);
            }
        }

        private void SafeBufferedDecompress(byte[] dataBuffer,
            int dataLength,
            ref byte[] outBuffer,
            out int outLength)
        {
            var service = CompressService.CreateCompressService(_header.CompressType);
            using (var inStream = new MemoryStream(dataBuffer, 0, dataLength)) {
                using (var outStream = new MemoryStream(outBuffer)) {
                    outStream.SetLength(0);
                    service.Decompress(inStream, outStream);
                    outLength = (int)outStream.Length;
                }
            }
            CompressService.DestroyCompressService(service);
        }


        #endregion

    }
}