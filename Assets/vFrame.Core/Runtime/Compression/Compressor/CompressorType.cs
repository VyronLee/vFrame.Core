// ------------------------------------------------------------
//         File: CompressorType.cs
//        Brief: Enumerates the supported compressor algorithms
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 22:55:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    /// <summary>
    ///     Identifies the compression algorithm to use.
    /// </summary>
    public enum CompressorType
    {
        Invalid = 0,
        LZMA = 1,
        LZ4 = 2,
        ZStd = 3,
        Zlib = 4
    }
}