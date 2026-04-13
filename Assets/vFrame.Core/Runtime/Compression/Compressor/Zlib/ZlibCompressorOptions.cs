// ------------------------------------------------------------
//         File: ZlibCompressorOptions.cs
//        Brief: Configuration options for the Zlib compressor.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-19 11:52:02
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using vFrame.Core.ThirdParty.Zlib;

namespace vFrame.Core
{
    public class ZlibCompressorOptions : CompressorOptions
    {
        /// <summary>
        ///     Gets or sets the Zlib compression level. Defaults to <see cref="CompressionLevel.BestCompression" />.
        /// </summary>
        public CompressionLevel Level { get; set; } = CompressionLevel.BestCompression;
    }
}