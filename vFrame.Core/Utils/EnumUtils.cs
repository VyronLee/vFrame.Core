//------------------------------------------------------------
//        File:  EnumUtils.cs
//       Brief:  Enum utils.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-09-09 17:01
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System;

namespace vFrame.Core.Utils
{
    public static class EnumUtils
    {
        public static int EnumIndex<T>(int value) {
            return EnumIndex(typeof(T), value);
        }

        public static int EnumIndex(Type type, int value) {
            var i = 0;
            foreach (var v in Enum.GetValues(type)) {
                if ((int) v == value)
                    return i;
                ++i;
            }

            return -1;
        }

        public static T FromString<T>(string str) {
            return (T) FromString(typeof(T), str);
        }

        public static object FromString(Type type, string str) {
            if (!Enum.IsDefined(type, str))
                throw new InvalidOperationException(
                    string.Format("No enum value defined in type: {0}, {1}", type.Name, str));
            return Enum.Parse(type, str);
        }
    }
}