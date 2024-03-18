using K4os.Compression.LZ4;

namespace vFrame.Core.Compress
{
    public class LZ4CompressorOptions : CompressorOptions
    {
        public LZ4Level Level { get; set; } = LZ4Level.L12_MAX;
    }
}