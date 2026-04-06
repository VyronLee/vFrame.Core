using System;

namespace vFrame.Core.ObjectPools
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

        public bool HasActiveObjects => CountActive > 0;

        public bool HasRetainedObjects => CountInactive > 0;

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
