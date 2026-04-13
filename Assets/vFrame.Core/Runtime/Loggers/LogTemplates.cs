// ------------------------------------------------------------
//         File: LogTemplates.cs
//        Brief: Preset log format template strings for common
//               output styles.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-11
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    /// <summary>
    ///     Provides preset log format template strings.
    /// </summary>
    public static class LogTemplates
    {
        /// <summary>
        ///     Default format: timestamp, level (3-char uppercase), tag, and message.
        ///     Example: [2026-04-11 21:18:00.123] [INF] [MyTag] Hello world
        /// </summary>
        public const string Default = "[{time}] [{level:u3}] [{tag}] {message}";

        /// <summary>
        ///     Compact format: level and message only.
        ///     Example: INF Hello world
        /// </summary>
        public const string Compact = "{level:u3} {message}";

        /// <summary>
        ///     Verbose format: timestamp, level, tag, thread, source line, and message.
        ///     Example: [2026-04-11 21:18:00.123] [INF] [MyTag] [T:1] [L:42] Hello world
        /// </summary>
        public const string Verbose = "[{time}] [{level:u3}] [{tag}] {thread} [L:{line}] {message}";
    }
}