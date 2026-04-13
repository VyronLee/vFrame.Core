// ------------------------------------------------------------
//         File: RollingStrategy.cs
//        Brief: Enumeration of file rolling strategies for
//               log file management.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-11
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    /// <summary>
    ///     Defines strategies for rolling (rotating) log files.
    /// </summary>
    public enum RollingStrategy
    {
        /// <summary>No file rolling; write to a single file indefinitely.</summary>
        None,

        /// <summary>Roll log files when they exceed the configured size limit.</summary>
        BySize,

        /// <summary>Roll log files on a date boundary (e.g., daily).</summary>
        ByDate
    }
}