// ------------------------------------------------------------
//         File: ITimer.cs
//        Brief: Contract for the timer system, providing delay, repeat, frame-based
//               scheduling, and lifecycle control (pause, resume, cancel).
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
    ///     Defines the public API for the timer system, allowing scheduled callbacks
    ///     with support for time-based and frame-based timers.
    /// </summary>
    public interface ITimer : IBaseObject
    {
        /// <summary>
        ///     Schedules a one-shot callback after the specified number of seconds.
        /// </summary>
        /// <param name="seconds">Delay duration in seconds.</param>
        /// <param name="callback">Action to invoke when the timer fires.</param>
        /// <returns>Unique identifier for the created timer.</returns>
        ulong Delay(float seconds, Action callback);

        /// <summary>
        ///     Schedules a one-shot callback after the specified number of frames.
        /// </summary>
        /// <param name="frameCount">Number of frames to wait before firing.</param>
        /// <param name="callback">Action to invoke when the timer fires.</param>
        /// <returns>Unique identifier for the created timer.</returns>
        ulong DelayFrame(int frameCount, Action callback);

        /// <summary>
        ///     Schedules a repeating callback at the given interval.
        /// </summary>
        /// <param name="interval">Time between invocations in seconds.</param>
        /// <param name="callback">Action to invoke on each tick.</param>
        /// <param name="count">Maximum number of invocations. Use -1 for infinite repetition.</param>
        /// <returns>Unique identifier for the created timer.</returns>
        ulong Repeat(float interval, Action callback, int count = -1);

        /// <summary>
        ///     Schedules a callback that fires every frame until cancelled.
        /// </summary>
        /// <param name="callback">Action to invoke each frame.</param>
        /// <returns>Unique identifier for the created timer.</returns>
        ulong RepeatEveryFrame(Action callback);

        /// <summary>
        ///     Cancels the timer with the specified identifier.
        /// </summary>
        /// <param name="timerId">Identifier of the timer to cancel.</param>
        void Cancel(ulong timerId);

        /// <summary>
        ///     Cancels all active timers managed by this instance.
        /// </summary>
        void CancelAll();

        /// <summary>
        ///     Pauses the timer with the specified identifier.
        /// </summary>
        /// <param name="timerId">Identifier of the timer to pause.</param>
        void Pause(ulong timerId);

        /// <summary>
        ///     Resumes a previously paused timer.
        /// </summary>
        /// <param name="timerId">Identifier of the timer to resume.</param>
        void Resume(ulong timerId);

        /// <summary>
        ///     Checks whether the specified timer is currently active and not paused.
        /// </summary>
        /// <param name="timerId">Identifier of the timer to query.</param>
        /// <returns><c>true</c> if the timer exists and is not paused; otherwise <c>false</c>.</returns>
        bool IsRunning(ulong timerId);

        /// <summary>
        ///     Gets the remaining time before the next invocation of the specified timer.
        /// </summary>
        /// <param name="timerId">Identifier of the timer to query.</param>
        /// <returns>Remaining time in seconds, or 0 if the timer does not exist.</returns>
        float GetRemainingTime(ulong timerId);

        /// <summary>
        ///     Advances all active timers by the given delta time. Must be called
        ///     by the consumer each frame (or at whatever granularity is desired).
        /// </summary>
        /// <param name="deltaTime">Time elapsed since the last update, in seconds.</param>
        void Update(float deltaTime);
    }
}
