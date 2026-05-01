// ------------------------------------------------------------
//         File: ObjectPool.cs
//        Brief: Unified object pool with policy-based creation and optional duplicate detection
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-07-09 19:09:00
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using System;
using System.Collections.Generic;

namespace vFrame.Core
{
    /// <summary>
    ///     Thread-safe generic object pool with policy-based object creation and optional duplicate detection.
    /// </summary>
    /// <typeparam name="T">The pooled object type, must be a reference type.</typeparam>
    public class ObjectPool<T> : IObjectPool<T>, IDisposable where T : class
    {
        private static readonly object _instanceLockObject = new object();
        private static ObjectPool<T> _shared;
        private readonly HashSet<T> _inactiveLookup; // null when CollectionCheckEnabled = false

        private readonly object _lockObject = new object();
        private readonly ObjectPoolOptions<T> _options;
        private readonly IPooledObjectPolicy<T> _policy;
        private Stack<T> _objects;
        private ObjectPoolStatistics _statistics;

        /// <summary>
        ///     Creates a pool with default policy and options.
        /// </summary>
        public ObjectPool() : this(default(IPooledObjectPolicy<T>)) { }

        /// <summary>
        ///     Creates a pool with default policy and custom configuration.
        /// </summary>
        /// <param name="options">Pool configuration options, or null for defaults.</param>
        public ObjectPool(ObjectPoolOptions<T> options) : this(default(IPooledObjectPolicy<T>), options) { }

        /// <summary>
        ///     Creates a pool with a factory function and optional configuration.
        /// </summary>
        /// <param name="factory">Function to create new instances, or null for default construction.</param>
        /// <param name="options">Pool configuration options, or null for defaults.</param>
        public ObjectPool(Func<T> factory, ObjectPoolOptions<T> options = null)
            : this(new DefaultPooledObjectPolicy<T>(factory), options) { }

        /// <summary>
        ///     Creates a pool with a custom policy and optional configuration.
        /// </summary>
        /// <param name="policy">Object creation and return policy, or null for default policy.</param>
        /// <param name="options">Pool configuration options, or null for defaults.</param>
        public ObjectPool(IPooledObjectPolicy<T> policy, ObjectPoolOptions<T> options = null) {
            _policy = policy ?? new DefaultPooledObjectPolicy<T>();
            _options = options ?? new ObjectPoolOptions<T>();
            _objects = new Stack<T>(_options.InitialCapacity);
            if (_options.CollectionCheckEnabled) {
                _inactiveLookup = new HashSet<T>();
            }
            else {
                _inactiveLookup = null;
            }
        }

        /// <summary>
        ///     Gets the lazily-initialized shared singleton instance.
        ///     Uses double-check lock pattern without calling Create().
        /// </summary>
        public static ObjectPool<T> Shared {
            get {
                if (null == _shared) {
                    lock (_instanceLockObject) {
                        if (null == _shared) {
                            _shared = new ObjectPool<T>();
                        }
                    }
                }

                return _shared;
            }
        }

        /// <summary>
        ///     Releases all resources used by the pool.
        /// </summary>
        public void Dispose() {
            Clear();
            _objects = null;
        }

        /// <summary>
        ///     Gets an object from the pool, creating a new instance via policy if none is available.
        /// </summary>
        /// <returns>A pooled or newly created instance.</returns>
        public T Get() {
            T item;
            lock (_lockObject) {
                if (_objects.Count > 0) {
                    item = _objects.Pop();
                    if (_inactiveLookup != null) {
                        _inactiveLookup.Remove(item);
                    }
                }
                else {
                    item = _policy.Create();
                    _statistics.CountAll++;
                    _statistics.TotalCreatedCount++;
                }

                _statistics.TotalGetCount++;
            }

            _options.OnGet?.Invoke(item);
            return item;
        }

        /// <summary>
        ///     Gets an object from the pool wrapped in an auto-returning disposable.
        /// </summary>
        /// <param name="item">The pooled object.</param>
        /// <returns>A PooledObject that returns the item to the pool when disposed.</returns>
        public PooledObject<T> Get(out T item) {
            item = Get();
            return new PooledObject<T>(this, item);
        }

        /// <summary>
        ///     Returns an object to the pool, applying policy checks, reset callbacks, and overflow handling.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        public void Return(T obj) {
            ThrowHelper.ThrowIfNull(obj, nameof(obj));

            // Step 1: Policy validation - if policy rejects, discard immediately
            if (!_policy.Return(obj)) {
                lock (_lockObject) {
                    _statistics.TotalReturnCount++;
                    _statistics.TotalDestroyedCount++;
                    _statistics.CountAll--;
                }

                _options.OnDestroy?.Invoke(obj);
                return;
            }

            // Step 2: Reset object state if it supports the interface
            if (obj is IPoolObjectResetable resetable) {
                resetable.Reset();
            }

            // Step 3: Invoke user return callback
            _options.OnReturn?.Invoke(obj);

            // Step 4: Check if object was destroyed during return callbacks
            if (obj is Object pooledObject && pooledObject.Destroyed) {
                lock (_lockObject) {
                    _statistics.TotalReturnCount++;
                    _statistics.TotalDestroyedCount++;
                    _statistics.CountAll--;
                }

                _options.OnDestroy?.Invoke(obj);
                return;
            }

            // Step 5: Add back to pool with duplicate and overflow checks
            lock (_lockObject) {
                // Duplicate detection (only when CollectionCheckEnabled is true)
                if (_inactiveLookup != null) {
                    if (_inactiveLookup.Contains(obj)) {
                        _statistics.TotalReturnCount++;
                        _statistics.TotalDuplicateReturnCount++;
                        return;
                    }

                    _inactiveLookup.Add(obj);
                }

                _statistics.TotalReturnCount++;

                // Overflow policy: destroy returned object if pool is at max capacity
                if (_objects.Count >= _options.MaxSize &&
                    _options.OverflowPolicy == ObjectPoolOverflowPolicy.DestroyReturned) {
                    _statistics.TotalDestroyedCount++;
                    _statistics.CountAll--;
                    if (_inactiveLookup != null) {
                        _inactiveLookup.Remove(obj);
                    }

                    _options.OnDestroy?.Invoke(obj);
                    return;
                }

                _objects.Push(obj);
            }
        }

        /// <summary>
        ///     Removes excess inactive objects from the pool, releasing them for garbage collection.
        /// </summary>
        /// <param name="maxRetained">Maximum number of inactive objects to retain.</param>
        /// <returns>The number of objects removed.</returns>
        public int Trim(int maxRetained) {
            var removed = 0;
            lock (_lockObject) {
                while (_objects.Count > maxRetained) {
                    var item = _objects.Pop();
                    if (_inactiveLookup != null) {
                        _inactiveLookup.Remove(item);
                    }

                    _statistics.CountAll--;
                    _statistics.TotalDestroyedCount++;
                    removed++;
                }
            }

            return removed;
        }

        /// <summary>
        ///     Returns a snapshot of current pool statistics.
        /// </summary>
        /// <returns>Current pool statistics.</returns>
        public ObjectPoolStatistics GetStatistics() {
            lock (_lockObject) {
                _statistics.CountInactive = _objects.Count;
                _statistics.CountActive = _statistics.CountAll - _statistics.CountInactive;
                return _statistics;
            }
        }

        /// <summary>
        ///     Clears all pooled objects, releasing them for garbage collection.
        /// </summary>
        public void Clear() {
            lock (_lockObject) {
                _objects.Clear();
                _inactiveLookup?.Clear();
            }
        }

        // IObjectPool explicit implementations

        /// <summary>
        ///     Non-generic get that boxes the result.
        /// </summary>
        object IObjectPool.Get() {
            return Get();
        }

        /// <summary>
        ///     Non-generic return with type validation.
        /// </summary>
        void IObjectPool.Return(object obj) {
            ThrowHelper.ThrowIfNull(obj, nameof(obj));
            ThrowHelper.ThrowIfTypeMismatch(obj.GetType(), typeof(T));
            Return(obj as T);
        }

        /// <summary>
        ///     Pre-populates the pool with a specified number of objects.
        /// </summary>
        /// <param name="count">Number of objects to create and add to the pool.</param>
        public void Prewarm(int count) {
            lock (_lockObject) {
                for (var i = 0; i < count; i++) {
                    var item = _policy.Create();
                    _objects.Push(item);
                    if (_inactiveLookup != null) {
                        _inactiveLookup.Add(item);
                    }

                    _statistics.CountAll++;
                    _statistics.TotalCreatedCount++;
                }
            }
        }
    }
}