// ------------------------------------------------------------
//         File: LoggerCategory.cs
//        Brief: Sealed category-based logger implementation
//               with per-category minimum level filtering.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-11
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Runtime.CompilerServices;

namespace vFrame.Core
{
    /// <summary>
    ///     A category-based logger that delegates to <see cref="Logger" /> static methods
    ///     with per-category minimum level filtering.
    /// </summary>
    public sealed class LoggerCategory : ILogger
    {
        private readonly LogTag _tag;

        /// <summary>
        ///     Creates a new logger category with the specified name.
        /// </summary>
        /// <param name="categoryName">The category name used as the log tag.</param>
        public LoggerCategory(string categoryName) {
            CategoryName = categoryName;
            _tag = new LogTag(categoryName);
        }

        public string CategoryName { get; }

        public LogLevelDef MinimumLevel { get; set; } = LogLevelDef.Trace;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEnabled(LogLevelDef level) {
            return MinimumLevel <= level && Logger.IsEnabled(level);
        }

        public void Trace(string text) {
            if (!IsEnabled(LogLevelDef.Trace)) {
                return;
            }

            Logger.Trace(_tag, text);
        }

        public void Debug(string text) {
            if (!IsEnabled(LogLevelDef.Debug)) {
                return;
            }

            Logger.Debug(_tag, text);
        }

        public void Info(string text) {
            if (!IsEnabled(LogLevelDef.Info)) {
                return;
            }

            Logger.Info(_tag, text);
        }

        public void Warning(string text) {
            if (!IsEnabled(LogLevelDef.Warning)) {
                return;
            }

            Logger.Warning(_tag, text);
        }

        public void Error(string text) {
            if (!IsEnabled(LogLevelDef.Error)) {
                return;
            }

            Logger.Error(_tag, text);
        }

        public void Fatal(string text) {
            if (!IsEnabled(LogLevelDef.Fatal)) {
                return;
            }

            Logger.Fatal(_tag, text);
        }

        public void Trace(Exception exception, string text = null) {
            if (!IsEnabled(LogLevelDef.Trace)) {
                return;
            }

            Logger.Trace(_tag, exception, text);
        }

        public void Debug(Exception exception, string text = null) {
            if (!IsEnabled(LogLevelDef.Debug)) {
                return;
            }

            Logger.Debug(_tag, exception, text);
        }

        public void Info(Exception exception, string text = null) {
            if (!IsEnabled(LogLevelDef.Info)) {
                return;
            }

            Logger.Info(_tag, exception, text);
        }

        public void Warning(Exception exception, string text = null) {
            if (!IsEnabled(LogLevelDef.Warning)) {
                return;
            }

            Logger.Warning(_tag, exception, text);
        }

        public void Error(Exception exception, string text = null) {
            if (!IsEnabled(LogLevelDef.Error)) {
                return;
            }

            Logger.Error(_tag, exception, text);
        }

        public void Fatal(Exception exception, string text = null) {
            if (!IsEnabled(LogLevelDef.Fatal)) {
                return;
            }

            Logger.Fatal(_tag, exception, text);
        }
    }
}