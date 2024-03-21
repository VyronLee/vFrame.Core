using System.Diagnostics;
using Logger = vFrame.Core.Loggers.Logger;

namespace vFrame.Core.Unity.SpawnPools
{
    internal static class SpawnPoolsDebug
    {
        [Conditional("DEBUG_SPAWNPOOLS")]
        public static void Log(string message, params object[] args) {
            Logger.Info(SpawnPoolsSettings.LogTag, message, args);
        }

        public static void Warning(string message, params object[] args) {
            Logger.Warning(SpawnPoolsSettings.LogTag, message, args);
        }

        public static void Error(string message, params object[] args) {
            Logger.Warning(SpawnPoolsSettings.LogTag, message, args);
        }
    }
}