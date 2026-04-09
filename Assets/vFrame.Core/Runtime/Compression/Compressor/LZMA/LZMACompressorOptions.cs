// ------------------------------------------------------------
//         File: LZMACompressorOptions.cs
//        Brief: Configuration options for the LZMA compressor (dictionary size and compression speed)
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 22:55:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    public class LZMACompressorOptions : CompressorOptions
    {
        /// <summary>
        /// LZMA dictionary size levels, ranging from 64 KiB to 64 MiB.
        /// </summary>
        public enum LZMADictionarySize
        {
            VerySmall = 1 << 16,
            Small = 1 << 20,
            Medium = 1 << 22,
            Large = 1 << 23,
            Larger = 1 << 24,
            VeryLarge = 1 << 26
        }

        /// <summary>
        /// LZMA compression speed levels.
        /// </summary>
        public enum LZMASpeed
        {
            Fastest = 5,
            VeryFast = 8,
            Fast = 16,
            Medium = 32,
            Slow = 64,
            VerySlow = 128
        }

        /// <summary>
        /// Gets or sets the LZMA compression speed.
        /// </summary>
        public LZMASpeed Speed { get; set; }

        /// <summary>
        /// Gets or sets the LZMA dictionary size.
        /// </summary>
        public LZMADictionarySize DictionarySize { get; set; }
    }
}
