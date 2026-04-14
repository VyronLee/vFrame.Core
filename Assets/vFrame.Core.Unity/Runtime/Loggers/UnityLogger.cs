//------------------------------------------------------------
//        File:  UnityLogger.cs
//       Brief:  Bridge that forwards core log events to the Unity Console
//               via the ILogSink interface.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2024-3-19 20:42
//   Copyright:  Copyright (c) 2024, VyronLee
//------------------------------------------------------------

using System;
using UnityEngine;
using vFrame.Core;

namespace vFrame.Core.Unity
{
    /// <summary>
    ///     Bridges the core logging system to the Unity Console.
    ///     Implements <see cref="Logger.ILogSink" /> for consistent per-sink level filtering.
    /// </summary>
    public sealed class UnityLogger : Logger.ILogSink
    {
        private static UnityLogger _instance;
        private static bool _opened;

        /// <summary>
        ///     Opens the Unity logging bridge over the core logging surface.
        ///     Registers itself as an <see cref="Logger.ILogSink" /> with the specified minimum level.
        /// </summary>
        /// <param name="level">The global log level for the core logger.</param>
        /// <param name="unityMinimumLevel">
        ///     The minimum level for this Unity Console sink. Logs below this level
        ///     are still processed by other sinks but not shown in the Unity Console.
        /// </param>
        /// <param name="logFile">Optional file path for file-based log output.</param>
        /// <param name="formatTemplate">Optional log format template string (e.g., <see cref="LogTemplates.Default" />).</param>
        public static void Open(LogLevelDef level,
            LogLevelDef unityMinimumLevel = LogLevelDef.Trace,
            string logFile = null,
            string formatTemplate = null) {
            if (_opened) {
                return;
            }

            _opened = true;

            Logger.LogLevel = level;

            if (!string.IsNullOrEmpty(formatTemplate)) {
                Logger.ApplyConfiguration(new LogConfiguration {
                    GlobalMinimumLevel = level,
                    FormatTemplate = formatTemplate
                });
            }

            Logger.LogFilePath = logFile;

            _instance = new UnityLogger();
            Logger.AddSink(_instance, unityMinimumLevel);

            Debug.unityLogger.filterLogType = level.ToUnityLogLevel();
        }

        /// <summary>
        ///     Closes the Unity logging bridge and removes the sink.
        ///     Does not affect other registered core sinks.
        /// </summary>
        public static void Close() {
            if (_instance != null) {
                Logger.RemoveSink(_instance);
                _instance = null;
            }

            Logger.Close();
            _opened = false;
        }

        /// <inheritdoc />
        public void OnLogReceived(Logger.LogContext context) {
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
