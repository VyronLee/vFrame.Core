// ------------------------------------------------------------
//         File: CoroutinePoolDebug.cs
//        Brief: Debug and diagnostic logging helpers for the
//               coroutine pool subsystem.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 22:09:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Diagnostics;

namespace vFrame.Core.Unity
{
    internal static class CoroutinePoolDebug
    {
        /// <summary>
        /// Logs an informational message when the DEBUG_COROUTINE_POOL
        /// compilation symbol is defined.
        /// </summary>
        /// <param name="message">Log message format string.</param>
        /// <param name="args">Optional format arguments.</param>
        [Conditional("DEBUG_COROUTINE_POOL")]
        public static void Log(string message, params object[] args) {
            Logger.Info(CoroutinePool.LogTag, string.Format(message, args));
        }

        /// <summary>
        /// Logs a warning message through the coroutine pool log channel.
        /// </summary>
        /// <param name="message">Log message format string.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Warning(string message, params object[] args) {
            Logger.Warning(CoroutinePool.LogTag, string.Format(message, args));
        }

        /// <summary>
        /// Logs an error message through the coroutine pool log channel.
        /// </summary>
        /// <param name="message">Log message format string.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Error(string message, params object[] args) {
            Logger.Error(CoroutinePool.LogTag, string.Format(message, args));
        }
    }
}