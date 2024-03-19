using vFrame.Core.ThirdParty.Zlib;

namespace vFrame.Core.Compression
{
    public class ZlibCompressorOptions : CompressorOptions
    {
        public CompressionLevel Level { get; set; } = CompressionLevel.BestCompression;
    }
}