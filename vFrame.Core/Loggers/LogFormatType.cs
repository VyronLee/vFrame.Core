//------------------------------------------------------------
//        File:  LogFormatType.cs
//       Brief:  Log format type definition
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-10-08 11:53
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

namespace vFrame.Core.Loggers
{
    public static class LogFormatType
    {
        public const int Tag = 1;
        public const int Time = 1 << 1;
        public const int Frame = 1 << 2;
        public const int Class = 1 << 3;
        public const int Function = 1 << 4;
    }
}