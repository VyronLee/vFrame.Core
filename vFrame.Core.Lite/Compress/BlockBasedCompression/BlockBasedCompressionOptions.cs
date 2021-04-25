using vFrame.Core.Compress.Services;

namespace vFrame.Core.Compress.BlockBasedCompression
{
    public class BlockBasedCompressionOptions
    {
        public CompressType CompressType { get; set; } = CompressType.LZMA;
        public int BlockSize { get; set; } = 1024; // default block size: 1k
    }
}