using System.Diagnostics;
using Logger = vFrame.Core.Loggers.Logger;

namespace vFrame.Core.Unity.SpawnPools
{
    public struct SpawnPoolsDiagnosticsSnapshot
    {
        public bool Enabled { get; set; }

        public int LogCount { get; set; }

        public int WarningCount { get; set; }

        public int ErrorCount { get; set; }
    }

    public static class SpawnPoolsDebug
    {
        private static SpawnPoolsDiagnosticsSnapshot _snapshot;

        public static SpawnPoolsDiagnosticsSnapshot GetSnapshot() {
            return _snapshot;
        }

        internal static void Configure(SpawnPoolsSettings settings) {
            _snapshot = new SpawnPoolsDiagnosticsSnapshot {
                Enabled = settings != null && settings.EnableDiagnostics
            };
        }

        internal static void Reset() {
            _snapshot = default;
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Log(string message, params object[] args) {
            if (!_snapshot.Enabled) {
                return;
            }

            _snapshot.LogCount++;
            Logger.Info(SpawnPoolsSettings.LogTag, message, args);
        }

        public static void Warning(string message, params object[] args) {
            if (_snapshot.Enabled) {
                _snapshot.WarningCount++;
            }

            Logger.Warning(SpawnPoolsSettings.LogTag, message, args);
        }

        public static void Error(string message, params object[] args) {
            if (_snapshot.Enabled) {
                _snapshot.ErrorCount++;
            }

            Logger.Error(SpawnPoolsSettings.LogTag, message, args);
        }
    }
}
