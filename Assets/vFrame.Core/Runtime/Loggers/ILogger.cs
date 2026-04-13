// ------------------------------------------------------------
//         File: ILogger.cs
//        Brief: Interface for category-based logger instances
//               that provide per-category log level filtering.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-11
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    /// <summary>
    ///     Interface for category-based logger instances with per-category log level filtering.
    ///     Zero-GC interpolated string support is provided via extension methods in
    ///     <see cref="LoggerExtensions" />.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        ///     The category name associated with this logger instance.
        /// </summary>
        string CategoryName { get; }

        /// <summary>
        ///     The minimum log level for this category. Messages below this level are suppressed.
        /// </summary>
        LogLevelDef MinimumLevel { get; set; }

        /// <summary>
        ///     Returns true if the given log level would produce output for this category.
        ///     Used internally by <see cref="LogInterpolatedStringHandler" /> for
        ///     compile-time short-circuit evaluation.
        /// </summary>
        bool IsEnabled(LogLevelDef level);

        void Trace(string text);
        void Debug(string text);
        void Info(string text);
        void Warning(string text);
        void Error(string text);
        void Fatal(string text);

        void Trace(Exception exception, string text = null);
        void Debug(Exception exception, string text = null);
        void Info(Exception exception, string text = null);
        void Warning(Exception exception, string text = null);
        void Error(Exception exception, string text = null);
        void Fatal(Exception exception, string text = null);
    }
}