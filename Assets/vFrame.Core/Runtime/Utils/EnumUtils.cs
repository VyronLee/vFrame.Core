//------------------------------------------------------------
//        File:  EnumUtils.cs
//       Brief:  Enum utils.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-09 17:01
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System;
using vFrame.Core.Exceptions;

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
                if ((int)v == value) {
                    return i;
                }
                ++i;
            }

            return -1;
        }

        public static T FromString<T>(string str) {
            return (T)FromString(typeof(T), str);
        }

        public static object FromString(Type type, string str) {
            if (!Enum.IsDefined(type, str)) {
                ThrowHelper.ThrowUndesiredException($"No enum value defined in type: {type.Name}, {str}");
            }
            return Enum.Parse(type, str);
        }
    }
}