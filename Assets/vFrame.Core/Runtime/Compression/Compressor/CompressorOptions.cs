// ------------------------------------------------------------
//         File: CompressorOptions.cs
//        Brief: Basic configuration options for compressors
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 22:55:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    public class CompressorOptions
    {
        /// <summary>
        ///     Gets or sets the internal buffer size in bytes used during compression and decompression.
        /// </summary>
        public int BuffSize { get; set; } = 81920;
    }
}