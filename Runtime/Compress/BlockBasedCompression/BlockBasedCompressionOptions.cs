namespace vFrame.Core.Compress
{
    public class BlockBasedCompressionOptions
    {
        public CompressorType CompressorType { get; set; } = CompressorType.LZMA;
        public CompressorOptions CompressOptions { get; set; }
        public int BlockSize { get; set; } = 1024; // default block size: 1k
    }
}