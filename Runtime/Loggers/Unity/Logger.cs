using System;
using UnityEngine;

namespace vFrame.Core.Loggers.Unity
{
    public static class UnityLogger
    {
        private static bool _opened;

        public static void Open(LogLevelDef level,
            string logFile = null,
            string logTagFormat = Logger.DefaultTagFormatter,
            int logFormatMask = Logger.DefaultLogFormatMask) {

            if (_opened) {
                return;
            }
            _opened = true;

            Logger.LogLevel = level;
            Logger.LogTagFormatter = logTagFormat;
            Logger.LogFormatMask = logFormatMask;
            Logger.LogFilePath = logFile;
            Logger.OnLogReceived += OnLogReceived;

            Debug.unityLogger.filterLogType = level.ToUnityLogLevel();
        }

        public static void Close() {
            Logger.Close();
            Logger.OnLogReceived -= OnLogReceived;
            _opened = false;
        }

        private static void OnLogReceived(Logger.LogContext context) {
            switch (context.Level) {
                case LogLevelDef.Debug:
                case LogLevelDef.Info:
                    Debug.Log(context.Content);
                    break;
                case LogLevelDef.Warning:
                    Debug.LogWarning(context.Content);
                    break;
                case LogLevelDef.Error:
                    Debug.LogError(context.Content);
                    break;
                case LogLevelDef.Fatal:
                    if (null != context.Exception) {
                        Debug.LogException(context.Exception);
                    }
                    else {
                        Debug.LogError(context.Content);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}