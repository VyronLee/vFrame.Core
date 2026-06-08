// ------------------------------------------------------------
//         File: Timer.cs
//        Brief: Standalone timer implementation that manages time-based and
//               frame-based scheduled callbacks with lifecycle support.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-06-07 00:00:00
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using System;
using System.Collections.Generic;

namespace vFrame.Core
{
    /// <summary>
    ///     Concrete timer that maintains an internal list of <see cref="TimerData"/>
    ///     entries and drives them via <see cref="Update"/> each frame.
    /// </summary>
    public class Timer : BaseObject, ITimer
    {
        private const int DefaultCapacity = 16;
        private ulong _nextId;
        private List<TimerData> _timers;

        // When true, Cancel sets Cancelled flag in-place instead of RemoveAt,
        // so the iteration loop sees the cancellation immediately.
        private bool _iterating;

        /// <inheritdoc/>
        public void Update(float deltaTime) {
            _iterating = true;

            // Reverse iteration for safe removal — list structure is stable
            // during iteration because Cancel only sets a flag.
            for (var i = _timers.Count - 1; i >= 0; i--) {
                var timer = _timers[i];

                // Skip paused or cancelled timers
                if (timer.Paused || timer.Cancelled) {
                    continue;
                }

                var shouldFire = false;

                if (timer.IsFrameBased) {
                    // Frame-based timer
                    timer.RemainingFrames--;
                    if (timer.RemainingFrames <= 0) {
                        shouldFire = true;
                    }
                }
                else {
                    // Time-based timer
                    timer.Remaining -= deltaTime;
                    if (timer.Remaining <= 0) {
                        shouldFire = true;
                    }
                }

                if (shouldFire) {
                    // Fire callback
                    timer.Callback?.Invoke();
                    timer.ExecutedCount++;

                    // Callback may have cancelled this timer — check before write-back
                    // to avoid overwriting the Cancelled flag.
                    if (_timers[i].Cancelled) {
                        continue;
                    }

                    // Check if should continue or stop
                    if (timer.RepeatCount == -1 || timer.ExecutedCount < timer.RepeatCount) {
                        // Reset for next iteration
                        if (timer.IsFrameBased) {
                            timer.RemainingFrames = timer.RepeatCount == -1 ? 0 : 1;
                        }
                        else {
                            timer.Remaining = timer.Interval;
                        }

                        _timers[i] = timer;
                    }
                    else {
                        // Remove completed timer
                        _timers.RemoveAt(i);
                    }
                }
                else {
                    // Update timer state
                    _timers[i] = timer;
                }
            }

            _iterating = false;

            // Sweep all cancelled timers
            SweepCancelled();
        }

        /// <inheritdoc/>
        public ulong Delay(float seconds, Action callback) {
            ThrowIfNotCreated();

            var timer = new TimerData {
                Id = _nextId++,
                Interval = seconds,
                Remaining = seconds,
                RepeatCount = 1,
                ExecutedCount = 0,
                Paused = false,
                IsFrameBased = false,
                Callback = callback
            };

            _timers.Add(timer);
            return timer.Id;
        }

        /// <inheritdoc/>
        public ulong DelayFrame(int frameCount, Action callback) {
            ThrowIfNotCreated();

            var timer = new TimerData {
                Id = _nextId++,
                Interval = 0,
                Remaining = 0,
                RepeatCount = 1,
                ExecutedCount = 0,
                Paused = false,
                IsFrameBased = true,
                RemainingFrames = frameCount,
                Callback = callback
            };

            _timers.Add(timer);
            return timer.Id;
        }

        /// <inheritdoc/>
        public ulong Repeat(float interval, Action callback, int count = -1) {
            ThrowIfNotCreated();

            var timer = new TimerData {
                Id = _nextId++,
                Interval = interval,
                Remaining = interval,
                RepeatCount = count,
                ExecutedCount = 0,
                Paused = false,
                IsFrameBased = false,
                Callback = callback
            };

            _timers.Add(timer);
            return timer.Id;
        }

        /// <inheritdoc/>
        public ulong RepeatEveryFrame(Action callback) {
            ThrowIfNotCreated();

            var timer = new TimerData {
                Id = _nextId++,
                Interval = 0,
                Remaining = 0,
                RepeatCount = -1,
                ExecutedCount = 0,
                Paused = false,
                IsFrameBased = true,
                RemainingFrames = 0,
                Callback = callback
            };

            _timers.Add(timer);
            return timer.Id;
        }

        /// <inheritdoc/>
        public void Cancel(ulong timerId) {
            ThrowIfNotCreated();

            if (_iterating) {
                // Mark in-place so iteration sees cancellation immediately
                for (var i = 0; i < _timers.Count; i++) {
                    if (_timers[i].Id == timerId) {
                        var timer = _timers[i];
                        timer.Cancelled = true;
                        _timers[i] = timer;
                        return;
                    }
                }
                return;
            }

            // Outside iteration: remove directly
            for (var i = 0; i < _timers.Count; i++) {
                if (_timers[i].Id == timerId) {
                    _timers.RemoveAt(i);
                    return;
                }
            }
        }

        /// <inheritdoc/>
        public void CancelAll() {
            ThrowIfNotCreated();

            if (_iterating) {
                // Mark all for immediate effect during iteration
                for (var i = 0; i < _timers.Count; i++) {
                    var timer = _timers[i];
                    timer.Cancelled = true;
                    _timers[i] = timer;
                }
                return;
            }

            _timers.Clear();
        }

        /// <inheritdoc/>
        public void Pause(ulong timerId) {
            ThrowIfNotCreated();

            for (var i = 0; i < _timers.Count; i++) {
                if (_timers[i].Id == timerId) {
                    var timer = _timers[i];
                    timer.Paused = true;
                    _timers[i] = timer;
                    return;
                }
            }
        }

        /// <inheritdoc/>
        public void Resume(ulong timerId) {
            ThrowIfNotCreated();

            for (var i = 0; i < _timers.Count; i++) {
                if (_timers[i].Id == timerId) {
                    var timer = _timers[i];
                    timer.Paused = false;
                    _timers[i] = timer;
                    return;
                }
            }
        }

        /// <inheritdoc/>
        public bool IsRunning(ulong timerId) {
            ThrowIfNotCreated();

            for (var i = 0; i < _timers.Count; i++) {
                if (_timers[i].Id == timerId) {
                    return !_timers[i].Paused && !_timers[i].Cancelled;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public float GetRemainingTime(ulong timerId) {
            ThrowIfNotCreated();

            for (var i = 0; i < _timers.Count; i++) {
                if (_timers[i].Id == timerId) {
                    return _timers[i].Cancelled ? 0f : _timers[i].Remaining;
                }
            }

            return 0f;
        }

        /// <inheritdoc/>
        protected override void OnCreate() {
            _timers = new List<TimerData>(DefaultCapacity);
            _nextId = 1;
        }

        /// <inheritdoc/>
        protected override void OnDestroy() {
            _timers?.Clear();
            _timers = null;
        }

        private void SweepCancelled() {
            for (var i = _timers.Count - 1; i >= 0; i--) {
                if (_timers[i].Cancelled) {
                    _timers.RemoveAt(i);
                }
            }
        }
    }
}
