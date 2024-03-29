//------------------------------------------------------------
//        File:  LogTag.cs
//       Brief:  Log tag definition.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-08 19:50
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System;

namespace vFrame.Core.Loggers
{
    public struct LogTag : IEquatable<LogTag>
    {
        private readonly string _name;

        public LogTag(string name = "undefined") {
            _name = name;
        }

        public override string ToString() {
            return _name;
        }

        public bool Equals(LogTag other) {
            return _name == other._name;
        }

        public override bool Equals(object obj) {
            return obj is LogTag other && Equals(other);
        }

        public override int GetHashCode() {
            return _name != null ? _name.GetHashCode() : 0;
        }
    }
}