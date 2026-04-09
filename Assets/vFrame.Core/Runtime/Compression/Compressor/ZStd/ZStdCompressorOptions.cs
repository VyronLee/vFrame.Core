// ------------------------------------------------------------
//         File: ZStdCompressorOptions.cs
//        Brief: Configuration options for the Zstandard compressor.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-19 11:52:02
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    public class ZStdCompressorOptions : CompressorOptions
    {
        /// <summary>
        /// Gets or sets the Zstandard compression level. Defaults to 11.
        /// </summary>
        public int Level { get; set; } = 11;
    }
}
