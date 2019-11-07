//------------------------------------------------------------
//        File:  TimeUtils.cs
//       Brief:  Time utils.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-09-09 17:15
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System;

namespace vFrame.Core.Utils
{
    public static class TimeUtils
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static double CurrentTimeInMilliSeconds() {
            return (DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
        }

        public static double CurrentTimeInSeconds() {
            return (DateTime.UtcNow - UnixEpoch).TotalSeconds;
        }
    }
}