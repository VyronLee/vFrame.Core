// ------------------------------------------------------------
//         File: LeakTrackingObjectPool.cs
//        Brief: Decorator that tracks rented objects not returned to the pool
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-15 19:09:00
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace vFrame.Core
{
    /// <summary>
    ///     Decorator that tracks rented objects to detect potential leaks.
    ///     Uses ConditionalWeakTable to track objects without preventing garbage collection.
    /// </summary>
    /// <typeparam name="T">The pooled object type (must be a reference type).</typeparam>
    public sealed class LeakTrackingObjectPool<T> : ObjectPoolDecorator<T> where T : class
    {
        private readonly ConditionalWeakTable<T, LeakInfo<T>> _leakTracker = new ConditionalWeakTable<T, LeakInfo<T>>();

        /// <summary>
        ///     Creates a new leak-tracking decorator wrapping the specified inner pool.
        /// </summary>
        /// <param name="inner">The inner pool to decorate.</param>
        public LeakTrackingObjectPool(IObjectPool<T> inner) : base(inner) { }

        /// <summary>
        ///     Gets an instance from the pool and tracks it for leak detection.
        /// </summary>
        /// <returns>A pooled object instance.</returns>
        public override T Get() {
            var item = base.Get();
            if (item != null) {
                _leakTracker.Remove(item);
                _leakTracker.Add(item, new LeakInfo<T>(item, DateTime.Now, new StackTrace(true).ToString()));
            }

            return item;
        }

        /// <summary>
        ///     Returns an instance to the pool and removes it from leak tracking.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        public override void Return(T obj) {
            if (obj != null) {
                _leakTracker.Remove(obj);
            }

            base.Return(obj);
        }

        /// <summary>
        ///     Gets the number of objects currently rented from the pool (potential leaks).
        /// </summary>
        /// <returns>The count of active objects.</returns>
        public int GetLeakCount() {
            return Inner.GetStatistics().CountActive;
        }
    }

    /// <summary>
    ///     Information about a potentially leaked object, including when it was rented and the stack trace.
    /// </summary>
    /// <typeparam name="T">The pooled object type.</typeparam>
    public sealed class LeakInfo<T> where T : class
    {
        /// <summary>
        ///     Creates a new leak info record.
        /// </summary>
        /// <param name="obj">The rented object.</param>
        /// <param name="rentTime">The time when the object was rented.</param>
        /// <param name="rentStackTrace">The stack trace at the time of rental.</param>
        public LeakInfo(T obj, DateTime rentTime, string rentStackTrace) {
            Object = obj;
            RentTime = rentTime;
            RentStackTrace = rentStackTrace;
        }

        /// <summary>
        ///     The rented object instance.
        /// </summary>
        public T Object { get; }

        /// <summary>
        ///     The time when the object was rented from the pool.
        /// </summary>
        public DateTime RentTime { get; }

        /// <summary>
        ///     The stack trace at the time of rental, showing where the object was rented.
        /// </summary>
        public string RentStackTrace { get; }
    }
}