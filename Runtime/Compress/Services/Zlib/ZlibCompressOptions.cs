using vFrame.Core.ThirdParty.Zlib;

namespace vFrame.Core.Compress.Services.Zlib
{
    public class ZlibCompressOptions : CompressServiceOptions
    {
        public CompressionLevel Level { get; set; } = CompressionLevel.BestCompression;
    }
}