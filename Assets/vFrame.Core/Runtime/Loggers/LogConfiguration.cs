// ------------------------------------------------------------
//         File: LogConfiguration.cs
//        Brief: Central configuration class for the logging
//               subsystem including levels, file options, and
//               format template.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-11
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;

namespace vFrame.Core
{
    /// <summary>
    ///     Central configuration for the logging subsystem.
    /// </summary>
    public class LogConfiguration
    {
        /// <summary>
        ///     The global minimum log level. Defaults to <see cref="LogLevelDef.Warning" />.
        /// </summary>
        public LogLevelDef GlobalMinimumLevel { get; set; } = LogLevelDef.Warning;

        /// <summary>
        ///     Per-category minimum log levels. Keys are category names, values are the minimum levels.
        /// </summary>
        public Dictionary<string, LogLevelDef> CategoryLevels { get; set; }

        /// <summary>
        ///     The file path for file-based log output.
        /// </summary>
        public string FileLogPath { get; set; }

        /// <summary>
        ///     Options for file-based log output including rolling strategy and limits.
        /// </summary>
        public LogToFileOptions FileLogOptions { get; set; }

        /// <summary>
        ///     A custom format template string for log output. Uses token syntax like {time}, {level}, etc.
        /// </summary>
        public string FormatTemplate { get; set; }

        /// <summary>
        ///     Whether to capture stack traces on every log call. Defaults to false.
        /// </summary>
        public bool CaptureStackTrace { get; set; }
    }
}