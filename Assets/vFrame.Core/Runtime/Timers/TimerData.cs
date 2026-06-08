// ------------------------------------------------------------
//         File: TimerData.cs
//        Brief: Internal value type holding all state for a single scheduled timer,
//               including timing, repetition, pause, and frame-based fields.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-06-07 00:00:00
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    /// <summary>
    ///     Mutable struct that stores the complete state for one scheduled timer entry.
    /// </summary>
    internal struct TimerData
    {
        /// <summary>
        ///     Unique identifier assigned to this timer.
        /// </summary>
        public ulong Id;

        /// <summary>
        ///     Interval between invocations for repeating timers, or the initial delay
        ///     for one-shot timers. Always in seconds for time-based timers.
        /// </summary>
        public float Interval;

        /// <summary>
        ///     Time remaining in seconds until the next invocation (time-based timers).
        /// </summary>
        public float Remaining;

        /// <summary>
        ///     Maximum number of invocations. A value of -1 means infinite repetition.
        /// </summary>
        public int RepeatCount;

        /// <summary>
        ///     Number of times the callback has been invoked so far.
        /// </summary>
        public int ExecutedCount;

        /// <summary>
        ///     Whether this timer is currently paused.
        /// </summary>
        public bool Paused;

        /// <summary>
        ///     When <c>true</c>, this timer counts frames instead of elapsed seconds.
        /// </summary>
        public bool IsFrameBased;

        /// <summary>
        ///     Frames remaining until the next invocation (frame-based timers).
        /// </summary>
        public int RemainingFrames;

        /// <summary>
        ///     Callback to invoke when the timer fires.
        /// </summary>
        public Action Callback;

        /// <summary>
        ///     Whether this timer has been cancelled and should be removed.
        ///     Set during iteration so the cancel takes effect immediately.
        /// </summary>
        public bool Cancelled;
    }
}
