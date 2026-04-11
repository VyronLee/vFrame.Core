//------------------------------------------------------------
//        File:  LogLevelExtension.cs
//       Brief:  Extension methods for converting core log levels to Unity LogType.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2024-3-19 20:42
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using UnityEngine;
using vFrame.Core;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Provides extension methods for converting core log levels to Unity LogType values.
    /// </summary>
    public static class LogLevelExtension
    {
        /// <summary>
        /// Converts a core LogLevelDef value to the equivalent Unity LogType.
        /// </summary>
        public static LogType ToUnityLogLevel(this LogLevelDef level) {
            switch (level) {
                case LogLevelDef.Trace:
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
                    ThrowHelper.ThrowUnsupportedEnum(level);
                    return LogType.Log;
            }
        }
    }
}
