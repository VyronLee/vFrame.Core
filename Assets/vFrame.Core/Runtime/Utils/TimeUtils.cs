// ------------------------------------------------------------
//         File: TimeUtils.cs
//        Brief: Utility class providing Unix timestamp retrieval
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-09 17:15
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    public static class TimeUtils
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        ///     Gets the current time as a Unix timestamp in milliseconds.
        /// </summary>
        /// <returns>The number of milliseconds since 1970-01-01 00:00:00 UTC.</returns>
        public static double CurrentTimeInMilliSeconds() {
            return (DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
        }

        /// <summary>
        ///     Gets the current time as a Unix timestamp in seconds.
        /// </summary>
        /// <returns>The number of seconds since 1970-01-01 00:00:00 UTC.</returns>
        public static double CurrentTimeInSeconds() {
            return (DateTime.UtcNow - UnixEpoch).TotalSeconds;
        }
    }
}