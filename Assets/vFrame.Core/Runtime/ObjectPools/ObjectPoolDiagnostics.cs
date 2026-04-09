// ------------------------------------------------------------
//         File: ObjectPoolDiagnostics.cs
//        Brief: Pool overflow policies, observable statistics, and configuration options
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-07-09 19:09:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    /// <summary>
    /// Determines how the pool reacts when a returned item would exceed retained capacity.
    /// </summary>
    public enum ObjectPoolOverflowPolicy
    {
        Retain,
        DestroyReturned
    }

    /// <summary>
    /// Observable pool diagnostics. The counters describe retention, destruction, reuse, and
    /// duplicate-return behavior without exposing internal storage details.
    /// </summary>
    public struct ObjectPoolStatistics
    {
        public int CountAll;
        public int CountInactive;
        public int CountActive;
        public int TotalGetCount;
        public int TotalReturnCount;
        public int TotalCreatedCount;
        public int TotalDestroyedCount;
        public int TotalDuplicateReturnCount;

        /// <summary>
        /// Gets whether the pool has objects currently in active use.
        /// </summary>
        public bool HasActiveObjects => CountActive > 0;

        /// <summary>
        /// Gets whether the pool has retained objects available for reuse.
        /// </summary>
        public bool HasRetainedObjects => CountInactive > 0;

        /// <summary>
        /// Gets whether any pool activity has been observed.
        /// </summary>
        public bool HasObservedActivity => TotalGetCount > 0 || TotalReturnCount > 0 || TotalCreatedCount > 0 ||
                                           TotalDestroyedCount > 0 || TotalDuplicateReturnCount > 0;
    }

    /// <summary>
    /// Lightweight policy hooks for retained object-pool behavior.
    /// </summary>
    public sealed class ObjectPoolOptions<TClass> where TClass : class
    {
        public int InitialCapacity { get; set; } = 128;
        public int MaxSize { get; set; } = 128;

        /// <summary>
        /// Applies when a returned item would exceed retained capacity.
        /// </summary>
        public ObjectPoolOverflowPolicy OverflowPolicy { get; set; } = ObjectPoolOverflowPolicy.DestroyReturned;

        /// <summary>
        /// Runs when an item is handed out for a new use cycle.
        /// </summary>
        public Action<TClass> OnGet { get; set; }

        /// <summary>
        /// Runs on return before pool-managed reset / retention policy completes.
        /// </summary>
        public Action<TClass> OnReturn { get; set; }

        /// <summary>
        /// Runs when pool policy destroys or discards a returned item instead of retaining it.
        /// </summary>
        public Action<TClass> OnDestroy { get; set; }
    }
}
