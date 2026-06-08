// ------------------------------------------------------------
//         File: TimerLifetimeExtensions.cs
//        Brief: Extension methods that bind scheduled timer lifetime to an
//               <see cref="ILifetime"/>, ensuring automatic cancellation on disposal.
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
    ///     Provides convenience extension methods that create a timer and register its
    ///     cancellation with an <see cref="ILifetime"/> so the timer is automatically
    ///     cancelled when the lifetime ends.
    /// </summary>
    public static class TimerLifetimeExtensions
    {
        /// <summary>
        ///     Schedules a delayed callback and binds its cancellation to the given lifetime.
        /// </summary>
        /// <param name="lifetime">Lifetime to bind the timer to.</param>
        /// <param name="timer">Timer used to create the delayed callback.</param>
        /// <param name="seconds">Delay duration in seconds.</param>
        /// <param name="callback">Action to invoke when the timer fires.</param>
        /// <returns>Unique identifier for the created timer.</returns>
        public static ulong Delay(this ILifetime lifetime, ITimer timer,
            float seconds, Action callback) {
            var id = timer.Delay(seconds, callback);
            lifetime.Add(() => timer.Cancel(id));
            return id;
        }

        /// <summary>
        ///     Schedules a repeating callback and binds its cancellation to the given lifetime.
        /// </summary>
        /// <param name="lifetime">Lifetime to bind the timer to.</param>
        /// <param name="timer">Timer used to create the repeating callback.</param>
        /// <param name="interval">Time between invocations in seconds.</param>
        /// <param name="callback">Action to invoke on each tick.</param>
        /// <param name="count">Maximum number of invocations. Use -1 for infinite repetition.</param>
        /// <returns>Unique identifier for the created timer.</returns>
        public static ulong Repeat(this ILifetime lifetime, ITimer timer,
            float interval, Action callback, int count = -1) {
            var id = timer.Repeat(interval, callback, count);
            lifetime.Add(() => timer.Cancel(id));
            return id;
        }

        /// <summary>
        ///     Schedules a frame-based delayed callback and binds its cancellation to the given lifetime.
        /// </summary>
        /// <param name="lifetime">Lifetime to bind the timer to.</param>
        /// <param name="timer">Timer used to create the frame-based callback.</param>
        /// <param name="frameCount">Number of frames to wait before firing.</param>
        /// <param name="callback">Action to invoke when the timer fires.</param>
        /// <returns>Unique identifier for the created timer.</returns>
        public static ulong DelayFrame(this ILifetime lifetime, ITimer timer,
            int frameCount, Action callback) {
            var id = timer.DelayFrame(frameCount, callback);
            lifetime.Add(() => timer.Cancel(id));
            return id;
        }
    }
}
