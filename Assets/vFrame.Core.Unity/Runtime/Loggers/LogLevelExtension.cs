using System;
using UnityEngine;
using vFrame.Core.Loggers;
using Logger = vFrame.Core.Loggers.Logger;

namespace vFrame.Core.Unity.Loggers
{
    public static class LogLevelExtension
    {
        public static LogType ToUnityLogLevel(this LogLevelDef level) {
            switch (Logger.LogLevel) {
                case LogLevelDef.Debug:
                case LogLevelDef.Info:
                    return LogType.Log;
                case LogLevelDef.Warning:
                    return LogType.Warning;
                case LogLevelDef.Error:
                    return LogType.Assert;
                case LogLevelDef.Fatal:
                    return LogType.Exception;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level));
            }
        }
    }
}