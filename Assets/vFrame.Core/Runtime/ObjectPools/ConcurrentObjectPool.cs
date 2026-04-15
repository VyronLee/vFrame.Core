// ------------------------------------------------------------
//         File: ConcurrentObjectPool.cs
//        Brief: Thread-safe object pool using ConcurrentStack for multi-threaded scenarios
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-12
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace vFrame.Core
{
    /// <summary>
    ///     Thread-safe object pool backed by <see cref="ConcurrentStack{T}" />.
    ///     Optimized for multi-threaded scenarios where objects are rented and returned
    ///     from different threads (e.g., background workers, parallel processing).
    /// </summary>
    /// <typeparam name="T">The pooled object type.</typeparam>
    public class ConcurrentObjectPool<T> : IObjectPool<T> where T : class
    {
        private readonly ConcurrentStack<T> _objects = new ConcurrentStack<T>();
        private readonly IPooledObjectPolicy<T> _policy;
        private readonly ObjectPoolOptions<T> _options;
        private int _countAll;
        private int _totalCreatedCount;
        private int _totalGetCount;
        private int _totalReturnCount;
        private int _totalDestroyedCount;

        /// <summary>
        ///     Initializes a new instance with default policy and options.
        /// </summary>
        public ConcurrentObjectPool() : this(default(IPooledObjectPolicy<T>))
        {
        }

        /// <summary>
        ///     Initializes a new instance with a factory function and optional options.
        /// </summary>
        /// <param name="factory">Function to create new instances when pool is empty.</param>
        /// <param name="options">Optional pool configuration.</param>
        public ConcurrentObjectPool(Func<T> factory, ObjectPoolOptions<T> options = null)
            : this(new DefaultPooledObjectPolicy<T>(factory), options)
        {
        }

        /// <summary>
        ///     Initializes a new instance with a custom policy and optional options.
        /// </summary>
        /// <param name="policy">Policy controlling object creation and return validation.</param>
        /// <param name="options">Optional pool configuration.</param>
        public ConcurrentObjectPool(IPooledObjectPolicy<T> policy, ObjectPoolOptions<T> options = null)
        {
            _policy = policy ?? new DefaultPooledObjectPolicy<T>();
            _options = options ?? new ObjectPoolOptions<T>();
        }

        /// <summary>
        ///     Gets an object from the pool, creating a new instance via policy if none is available.
        /// </summary>
        /// <returns>A pooled or newly created instance.</returns>
        public T Get()
        {
            T item = null;

            if (_objects.TryPop(out var pooled))
            {
                item = pooled;
            }

            if (item == null)
            {
                item = _policy.Create();
                Interlocked.Increment(ref _countAll);
                Interlocked.Increment(ref _totalCreatedCount);
            }

            Interlocked.Increment(ref _totalGetCount);

            if (_options.OnGet != null)
            {
                _options.OnGet(item);
            }

            return item;
        }

        /// <summary>
        ///     Gets an object from the pool wrapped in a <see cref="PooledObject{T}" /> for automatic return.
        /// </summary>
        /// <param name="item">The pooled or newly created instance.</param>
        /// <returns>A disposable struct that returns the object to the pool.</returns>
        public PooledObject<T> Get(out T item)
        {
            item = Get();
            return new PooledObject<T>(this, item);
        }

        /// <summary>
        ///     Returns an object to the pool after validation and optional reset.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        public void Return(T obj)
        {
            // 1. Null check
            if (obj == null)
            {
                return;
            }

            // 2. Policy validation
            if (!_policy.Return(obj))
            {
                Interlocked.Increment(ref _totalDestroyedCount);
                _options.OnDestroy?.Invoke(obj);
                return;
            }

            // 3. Reset if applicable
            if (obj is IPoolObjectResetable resetable)
            {
                resetable.Reset();
            }

            // 4. OnReturn callback
            _options.OnReturn?.Invoke(obj);

            // 5. Unity Object destroyed check
            if (obj is Object unityObj && unityObj.Destroyed)
            {
                Interlocked.Increment(ref _totalDestroyedCount);
                _options.OnDestroy?.Invoke(obj);
                return;
            }

            // 6. Overflow check
            if (_options.MaxSize > 0 && _objects.Count >= _options.MaxSize &&
                _options.OverflowPolicy == ObjectPoolOverflowPolicy.DestroyReturned)
            {
                Interlocked.Increment(ref _totalDestroyedCount);
                _options.OnDestroy?.Invoke(obj);
                return;
            }

            // 7. Push to pool
            _objects.Push(obj);
            Interlocked.Increment(ref _totalReturnCount);
        }

        /// <summary>
        ///     Removes excess items from the pool, retaining at most <paramref name="maxRetained" />.
        /// </summary>
        /// <param name="maxRetained">Maximum number of items to retain in the pool.</param>
        /// <returns>The number of items destroyed.</returns>
        public int Trim(int maxRetained)
        {
            var destroyed = 0;

            while (_objects.Count > maxRetained && _objects.TryPop(out var obj))
            {
                Interlocked.Increment(ref _totalDestroyedCount);
                _options.OnDestroy?.Invoke(obj);
                destroyed++;
            }

            return destroyed;
        }

        /// <summary>
        ///     Returns a snapshot of current pool statistics using thread-safe counter reads.
        /// </summary>
        /// <returns>Current pool statistics.</returns>
        public ObjectPoolStatistics GetStatistics()
        {
            return new ObjectPoolStatistics
            {
                CountAll = Interlocked.CompareExchange(ref _countAll, 0, 0),
                CountInactive = _objects.Count,
                CountActive = Interlocked.CompareExchange(ref _countAll, 0, 0) - _objects.Count,
                TotalGetCount = Interlocked.CompareExchange(ref _totalGetCount, 0, 0),
                TotalReturnCount = Interlocked.CompareExchange(ref _totalReturnCount, 0, 0),
                TotalCreatedCount = Interlocked.CompareExchange(ref _totalCreatedCount, 0, 0),
                TotalDestroyedCount = Interlocked.CompareExchange(ref _totalDestroyedCount, 0, 0)
            };
        }

        /// <summary>
        ///     Clears all objects from the pool, destroying them via the configured callback.
        /// </summary>
        public void Clear()
        {
            while (_objects.TryPop(out var obj))
            {
                Interlocked.Increment(ref _totalDestroyedCount);
                _options.OnDestroy?.Invoke(obj);
            }
        }

        // Explicit IObjectPool implementations
        object IObjectPool.Get() => Get();

        void IObjectPool.Return(object obj) => Return(obj as T);

        PooledObject<T> IObjectPool<T>.Get(out T item) => Get(out item);

        void IObjectPool<T>.Clear() => Clear();

        ObjectPoolStatistics IObjectPool.GetStatistics() => GetStatistics();

        int IObjectPool.Trim(int maxRetained) => Trim(maxRetained);
    }
}
