using System.Diagnostics;
using vFrame.Core.Loggers;

namespace vFrame.Core.Unity.SpawnPools
{
    internal static class SpawnPoolDebug
    {
        [Conditional("DEBUG_SPAWNPOOLS")]
        public static void Log(string message, params object[] args) {
            Logger.Info(SpawnPoolsSetting.LogTag, message, args);
        }
        
        public static void Warning(string message, params object[] args) {
            Logger.Warning(SpawnPoolsSetting.LogTag, message, args);
        }
        
        public static void Error(string message, params object[] args) {
            Logger.Warning(SpawnPoolsSetting.LogTag, message, args);
        }
    }
}