//------------------------------------------------------------
//        File:  UnityLogger.cs
//       Brief:  Bridge that forwards core log events to the Unity Console.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2024-3-19 20:42
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System;
using UnityEngine;
using vFrame.Core;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Bridges the core logging system to the Unity Console.
    /// </summary>
    public static class UnityLogger
    {
        private static bool _opened;

        /// <summary>
        /// Opens the Unity logging bridge over the core logging surface.
        /// Unity remains one consumer of core log emission rather than the whole logging system.
        /// </summary>
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

        /// <summary>
        /// Closes the Unity logging bridge without affecting other registered core sinks.
        /// </summary>
        public static void Close() {
            Logger.Close();
            Logger.OnLogReceived -= OnLogReceived;
            _opened = false;
        }

        private static void OnLogReceived(Logger.LogContext context) {
            // Include tag for console filtering
            var tag = context.Tag.ToString() != "undefined"
                ? $"[{context.Tag}] "
                : "";

            // If an exception is present at any level, log it as an exception
            if (context.Exception != null) {
                Debug.LogException(context.Exception);
                return;
            }

            switch (context.Level) {
                case LogLevelDef.Trace:
                case LogLevelDef.Debug:
                case LogLevelDef.Info:
                    Debug.Log($"{tag}{context.Content}");
                    break;
                case LogLevelDef.Warning:
                    Debug.LogWarning($"{tag}{context.Content}");
                    break;
                case LogLevelDef.Error:
                    Debug.LogError($"{tag}{context.Content}");
                    break;
                case LogLevelDef.Fatal:
                    Debug.LogError($"[FATAL] {tag}{context.Content}");
                    break;
                default:
                    ThrowHelper.ThrowUnsupportedEnum(context.Level);
                    break;
            }
        }
    }
}