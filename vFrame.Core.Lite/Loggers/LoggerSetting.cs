//------------------------------------------------------------
//        File:  LoggerSetting.cs
//       Brief:  日志设置
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2018-10-20 18:10
//   Copyright:  Copyright (c) 2018, VyronLee
//============================================================

namespace vFrame.Core.Loggers
{
    public static class LoggerSetting
    {
        public static int Capacity = 1000;

        public static string TagFormatter = "#{0}#";

        public static int LogFormatMask = LogFormatType.Tag |
                                          LogFormatType.Time |
                                          LogFormatType.Class |
                                          LogFormatType.Function;

        public static readonly int DefaultLogFormatMask = LogFormatType.Tag |
                                                          LogFormatType.Time |
                                                          LogFormatType.Class |
                                                          LogFormatType.Function;
    }
}