//------------------------------------------------------------
//        File:  LogLevelDef.cs
//       Brief:  日志等级定义
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2018-10-20 18:06
//   Copyright:  Copyright (c) 2018, VyronLee
//============================================================

using System;

namespace vFrame.Core.Loggers
{
    public enum LogLevelDef
    {
        Debug = 1,
        Info = 2,
        Warning = 4,
        Error = 8,
        Fatal = 16,
    }

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
                    throw new ArgumentOutOfRangeException("level");
            }
        }
    }
}