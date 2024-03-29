using System.Diagnostics;
using Logger = vFrame.Core.Loggers.Logger;

namespace vFrame.Core.Unity.Coroutine
{
    internal static class CoroutinePoolDebug
    {
        [Conditional("DEBUG_COROUTINE_POOL")]
        public static void Log(string message, params object[] args) {
            Logger.Info(CoroutinePool.LogTag, message, args);
        }

        public static void Warning(string message, params object[] args) {
            Logger.Warning(CoroutinePool.LogTag, message, args);
        }

        public static void Error(string message, params object[] args) {
            Logger.Warning(CoroutinePool.LogTag, message, args);
        }
    }
}