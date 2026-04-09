// ------------------------------------------------------------
//         File: LogLevelDef.cs
//        Brief: Bitmask enumeration defining supported log
//               severity levels.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2018-10-20 18:06:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    public enum LogLevelDef
    {
        Debug = 1,
        Info = 2,
        Warning = 4,
        Error = 8,
        Fatal = 16
    }
}
