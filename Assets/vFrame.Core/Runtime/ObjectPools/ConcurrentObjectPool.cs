// ------------------------------------------------------------
//         File: ConcurrentObjectPool.cs
//        Brief: Thread-safe object pool using ConcurrentBag for multi-threaded scenarios
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-12
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using System.Collections.Concurrent;
using System.Threading;

namespace vFrame.Core
{
    /// <summary>
    ///     Thread-safe object pool backed by <see cref="ConcurrentBag{T}" />.
    ///     Optimized for multi-threaded scenarios where objects are rented and returned
    ///     from different threads (e.g., background workers, parallel processing).
    /// </summary>
    /// <typeparam name="TClass">The pooled object type.</typeparam>
    /// <typeparam name="TAllocator">The allocator type for creating and resetting instances.</typeparam>
    public class ConcurrentObjectPool<TClass, TAllocator> : IObjectPool<TClass>
        where TClass : class, new()
        where TAllocator : IPoolObjectAllocator<TClass>, new()
    {
        private readonly TAllocator _allocator = new TAllocator();
        private readonly ConcurrentBag<TClass> _objects = new ConcurrentBag<TClass>();
        private int _countAll;
        private int _totalCreatedCount;
        private int _totalGetCount;
        private int _totalReturnCount;

        /// <summary>
        ///     Gets an object from the pool, allocating a new instance if none is available.
        /// </summary>
        /// <returns>A pooled or newly allocated instance.</returns>
        public TClass Get() {
            if (_objects.TryTake(out var item)) {
                Interlocked.Increment(ref _totalGetCount);
                return item;
            }

            var newItem = _allocator.Alloc();
            Interlocked.Increment(ref _countAll);
            Interlocked.Increment(ref _totalCreatedCount);
            Interlocked.Increment(ref _totalGetCount);
            return newItem;
        }

        /// <summary>
        ///     Returns an object to the pool after resetting it.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        public void Return(TClass obj) {
            if (obj == null) {
                return;
            }

            _allocator.Reset(obj);
            _objects.Add(obj);
            Interlocked.Increment(ref _totalReturnCount);
        }

        /// <summary>
        ///     Returns pool statistics snapshot. Note: CountInactive/CountActive are approximate
        ///     under high concurrency.
        /// </summary>
        /// <returns>Current pool statistics.</returns>
        public ObjectPoolStatistics GetStatistics() {
            return new ObjectPoolStatistics {
                CountAll = _countAll,
                CountInactive = _objects.Count,
                CountActive = _countAll - _objects.Count,
                TotalGetCount = _totalGetCount,
                TotalReturnCount = _totalReturnCount,
                TotalCreatedCount = _totalCreatedCount
            };
        }

        /// <summary>
        ///     Not supported for ConcurrentObjectPool. Returns 0.
        ///     Use GC or allocate-on-demand semantics instead.
        /// </summary>
        public int Trim(int maxRetained) {
            return 0;
        }

        object IObjectPool.Get() {
            return Get();
        }

        void IObjectPool.Return(object obj) {
            Return(obj as TClass);
        }
    }

    /// <summary>
    ///     Thread-safe object pool backed by <see cref="ConcurrentBag{T}" /> using default construction.
    /// </summary>
    /// <typeparam name="TClass">The pooled object type, must have a parameterless constructor.</typeparam>
    public class ConcurrentObjectPool<TClass> : IObjectPool<TClass>
        where TClass : class, new()
    {
        private readonly ConcurrentBag<TClass> _objects = new ConcurrentBag<TClass>();
        private int _countAll;
        private int _totalCreatedCount;
        private int _totalGetCount;
        private int _totalReturnCount;

        /// <summary>
        ///     Gets an object from the pool, allocating a new instance if none is available.
        /// </summary>
        /// <returns>A pooled or newly created instance.</returns>
        public TClass Get() {
            if (_objects.TryTake(out var item)) {
                Interlocked.Increment(ref _totalGetCount);
                return item;
            }

            var newItem = new TClass();
            Interlocked.Increment(ref _countAll);
            Interlocked.Increment(ref _totalCreatedCount);
            Interlocked.Increment(ref _totalGetCount);
            return newItem;
        }

        /// <summary>
        ///     Returns an object to the pool. If the object implements <see cref="IPoolObjectResetable" />,
        ///     <see cref="IPoolObjectResetable.Reset" /> is called before returning.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        public void Return(TClass obj) {
            if (obj == null) {
                return;
            }

            if (obj is IPoolObjectResetable resetable) {
                resetable.Reset();
            }

            _objects.Add(obj);
            Interlocked.Increment(ref _totalReturnCount);
        }

        /// <summary>
        ///     Returns pool statistics snapshot. Note: CountInactive/CountActive are approximate
        ///     under high concurrency.
        /// </summary>
        /// <returns>Current pool statistics.</returns>
        public ObjectPoolStatistics GetStatistics() {
            return new ObjectPoolStatistics {
                CountAll = _countAll,
                CountInactive = _objects.Count,
                CountActive = _countAll - _objects.Count,
                TotalGetCount = _totalGetCount,
                TotalReturnCount = _totalReturnCount,
                TotalCreatedCount = _totalCreatedCount
            };
        }

        /// <summary>
        ///     Not supported for ConcurrentObjectPool. Returns 0.
        /// </summary>
        public int Trim(int maxRetained) {
            return 0;
        }

        object IObjectPool.Get() {
            return Get();
        }

        void IObjectPool.Return(object obj) {
            Return(obj as TClass);
        }
    }
}