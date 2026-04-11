// ------------------------------------------------------------
//         File: SpawnPoolsDebug.cs
//        Brief: Diagnostics helper for spawn pools; tracks log,
//                warning, and error counts and provides conditional
//                logging gated by the diagnostics setting.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 23:47:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Diagnostics;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Immutable snapshot capturing current diagnostics counters for spawn pools.
    /// </summary>
    public struct SpawnPoolsDiagnosticsSnapshot
    {
        /// <summary>
        /// Gets or sets whether diagnostics logging is currently enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the total number of diagnostic log messages emitted.
        /// </summary>
        public int LogCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of warning messages emitted.
        /// </summary>
        public int WarningCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of error messages emitted.
        /// </summary>
        public int ErrorCount { get; set; }
    }

    /// <summary>
    /// Static diagnostics facade for spawn pools. Provides conditional logging
    /// and tracks message counters when diagnostics are enabled.
    /// </summary>
    public static class SpawnPoolsDebug
    {
        private static SpawnPoolsDiagnosticsSnapshot _snapshot;

        /// <summary>
        /// Returns the current diagnostics snapshot.
        /// </summary>
        /// <returns>A <see cref="SpawnPoolsDiagnosticsSnapshot"/> containing current counters.</returns>
        public static SpawnPoolsDiagnosticsSnapshot GetSnapshot() {
            return _snapshot;
        }

        /// <summary>
        /// Configures diagnostics state from the supplied settings instance.
        /// </summary>
        /// <param name="settings">Settings whose <c>EnableDiagnostics</c> flag drives logging.</param>
        internal static void Configure(SpawnPoolsSettings settings) {
            _snapshot = new SpawnPoolsDiagnosticsSnapshot {
                Enabled = settings != null && settings.EnableDiagnostics
            };
        }

        /// <summary>
        /// Resets all diagnostics counters and disables logging.
        /// </summary>
        internal static void Reset() {
            _snapshot = default;
        }

        /// <summary>
        /// Emits a diagnostic info message when diagnostics are enabled.
        /// Only active in the Unity Editor or development builds.
        /// </summary>
        /// <param name="message">Format string for the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Log(string message, params object[] args) {
            if (!_snapshot.Enabled) {
                return;
            }

            _snapshot.LogCount++;
            Logger.Info(SpawnPoolsSettings.LogTag, string.Format(message, args));
        }

        /// <summary>
        /// Emits a warning message. The counter is incremented only when diagnostics are enabled.
        /// </summary>
        /// <param name="message">Format string for the warning message.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Warning(string message, params object[] args) {
            if (_snapshot.Enabled) {
                _snapshot.WarningCount++;
            }

            Logger.Warning(SpawnPoolsSettings.LogTag, string.Format(message, args));
        }

        /// <summary>
        /// Emits an error message. The counter is incremented only when diagnostics are enabled.
        /// </summary>
        /// <param name="message">Format string for the error message.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Error(string message, params object[] args) {
            if (_snapshot.Enabled) {
                _snapshot.ErrorCount++;
            }

            Logger.Error(SpawnPoolsSettings.LogTag, string.Format(message, args));
        }
    }
}