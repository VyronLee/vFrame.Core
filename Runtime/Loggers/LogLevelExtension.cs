using System;
using vFrame.Core.Loggers;

namespace vFrame.Core.Unity.Loggers
{
    public static class LogLevelExtension
    {
        public static UnityEngine.LogType ToUnityLogLevel(this LogLevelDef level) {
            switch (Logger.LogLevel) {
                case LogLevelDef.Debug:
                case LogLevelDef.Info:
                    return UnityEngine.LogType.Log;
                case LogLevelDef.Warning:
                    return UnityEngine.LogType.Warning;
                case LogLevelDef.Error:
                    return UnityEngine.LogType.Assert;
                case LogLevelDef.Fatal:
                    return UnityEngine.LogType.Exception;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level));
            }
        }
    }
}