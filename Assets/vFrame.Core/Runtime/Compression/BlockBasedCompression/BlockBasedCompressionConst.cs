// ------------------------------------------------------------
//         File: BlockBasedCompressionConst.cs
//        Brief: Constants for block-based compression file format (magic ID and version)
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 22:55:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    public class BlockBasedCompressionConst
    {
        public const long Id = 0x766672616d65;

        public const long Version = 0x3;
    }
}