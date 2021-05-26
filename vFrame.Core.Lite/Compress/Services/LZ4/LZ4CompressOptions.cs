using K4os.Compression.LZ4;

namespace vFrame.Core.Compress.Services.LZ4
{
    public class LZ4CompressOptions : CompressServiceOptions
    {
        public LZ4Level Level { get; set; } = LZ4Level.L12_MAX;
    }
}