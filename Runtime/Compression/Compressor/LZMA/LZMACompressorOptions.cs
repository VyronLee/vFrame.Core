namespace vFrame.Core.Compression
{
    public class LZMACompressorOptions : CompressorOptions
    {
        public enum LZMASpeed
        {
            Fastest = 5,
            VeryFast = 8,
            Fast = 16,
            Medium = 32,
            Slow = 64,
            VerySlow = 128,
        }

        public enum LZMADictionarySize
        {
            ///<summary>64 KiB</summary>
            VerySmall = 1 << 16,

            ///<summary>1 MiB</summary>
            Small = 1 << 20,

            ///<summary>4 MiB</summary>
            Medium = 1 << 22,

            ///<summary>8 MiB</summary>
            Large = 1 << 23,

            ///<summary>16 MiB</summary>
            Larger = 1 << 24,

            ///<summary>64 MiB</summary>
            VeryLarge = 1 << 26,
        }

        public LZMASpeed Speed { get; set; }
        public LZMADictionarySize DictionarySize { get; set; }
    }
}