// ------------------------------------------------------------
//         File: LZ4CompressorOptions.cs
//        Brief: Configuration options for the LZ4 compressor
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 22:55:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using K4os.Compression.LZ4;

namespace vFrame.Core
{
    public class LZ4CompressorOptions : CompressorOptions
    {
        /// <summary>
        /// Gets or sets the LZ4 compression level.
        /// </summary>
        public LZ4Level Level { get; set; } = LZ4Level.L12_MAX;
    }
}
