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
        public static int EnumIndex<T>(int value)
        {
            var i = 0;
            foreach (var v in Enum.GetValues(typeof(T)))
            {
                if ((int) v == value)
                    return i;
                ++i;
            }

            return -1;
        }

        public static T FromString<T>(string str)
        {
            if (!Enum.IsDefined(typeof(T), str))
                return default(T);
            return (T) Enum.Parse(typeof(T), str);
        }
    }
}