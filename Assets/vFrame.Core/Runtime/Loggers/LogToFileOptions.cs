// ------------------------------------------------------------
//         File: LogToFileOptions.cs
//        Brief: Configuration options for file-based log sinks
//               including rolling strategy and flush interval.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-11
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    /// <summary>
    ///     Configuration options for file-based log output with rolling support.
    /// </summary>
    public class LogToFileOptions
    {
        /// <summary>
        ///     The base file path for log output.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        ///     The rolling strategy to use for log file rotation. Defaults to <see cref="RollingStrategy.ByDate" />.
        /// </summary>
        public RollingStrategy Strategy { get; set; } = RollingStrategy.ByDate;

        /// <summary>
        ///     Maximum file size in bytes before rolling (when <see cref="Strategy" /> is <see cref="RollingStrategy.BySize" />).
        ///     Defaults to 50 MB.
        /// </summary>
        public long MaxFileSizeBytes { get; set; } = 50 * 1024 * 1024;

        /// <summary>
        ///     Maximum number of archived log files to retain. Defaults to 7.
        /// </summary>
        public int MaxFileCount { get; set; } = 7;

        /// <summary>
        ///     Whether to compress archived log files. Defaults to false.
        ///     When true, uses the algorithm specified by <see cref="CompressionType" />.
        /// </summary>
        public bool CompressArchives { get; set; } = false;

        /// <summary>
        ///     The compression algorithm to use when <see cref="CompressArchives" /> is true.
        ///     Defaults to <see cref="CompressorType.ZStd" />.
        /// </summary>
        public CompressorType CompressionType { get; set; } = CompressorType.ZStd;

        /// <summary>
        ///     Interval in seconds between automatic flushes to disk. Defaults to 1 second.
        /// </summary>
        public int FlushIntervalSeconds { get; set; } = 1;
    }
}