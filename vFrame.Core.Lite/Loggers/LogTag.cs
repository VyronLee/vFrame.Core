//------------------------------------------------------------
//        File:  LogTag.cs
//       Brief:  Log tag definition.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-09-08 19:50
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

namespace vFrame.Core.Loggers
{
    public struct LogTag
    {
        private readonly string _name;

        public LogTag(string name = "undefined") {
            _name = name;
        }

        public override string ToString() {
            return _name;
        }
    }
}