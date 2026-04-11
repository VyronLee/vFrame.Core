// ------------------------------------------------------------
//         File: LogFormatType.cs
//        Brief: Bitmask constants for configuring which fields
//               appear in formatted log output.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-10-08 11:53:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    public static class LogFormatType
    {
        public const int Tag = 1;
        public const int Time = 1 << 1;
        public const int Class = 1 << 2;
        public const int Function = 1 << 3;
        public const int Thread = 1 << 4;
        public const int Line = 1 << 5;
    }
}
