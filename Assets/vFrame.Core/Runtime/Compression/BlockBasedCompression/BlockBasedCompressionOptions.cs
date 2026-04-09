// ------------------------------------------------------------
//         File: BlockBasedCompressionOptions.cs
//        Brief: Configuration options for block-based compression (compressor type, block size, etc.)
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 22:55:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    public class BlockBasedCompressionOptions
    {
        public CompressorType CompressorType { get; set; } = CompressorType.LZMA;
        public CompressorOptions CompressOptions { get; set; }
        public int BlockSize { get; set; } = 1024;
    }
}
