//------------------------------------------------------------
//        File:  LogLevelDef.cs
//       Brief:  日志等级定义
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2018-10-20 18:06
//   Copyright:  Copyright (c) 2018, VyronLee
//============================================================
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
}