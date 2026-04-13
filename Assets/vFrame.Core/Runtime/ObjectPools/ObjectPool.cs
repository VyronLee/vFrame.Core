// ------------------------------------------------------------
//         File: ObjectPool.cs
//        Brief: Generic object pool implementation supporting default construction and custom allocator modes
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-07-09 19:09:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;

namespace vFrame.Core
{
    public abstract class ObjectPool : BaseObject, IObjectPool
    {
        protected readonly LogTag LogTag = new LogTag("ObjectPool");

        /// <summary>
        ///     Starts a new pooled use cycle.
        /// </summary>
        /// <returns>An object from the pool.</returns>
        public object Get() {
            return OnGetInternal();
        }

        /// <summary>
        ///     Ends the current pooled use cycle and applies pool-managed return policy.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        public void Return(object obj) {
            OnReturnInternal(obj);
        }

        /// <summary>
        ///     Returns observable pool statistics.
        /// </summary>
        /// <returns>Current pool statistics snapshot.</returns>
        public abstract ObjectPoolStatistics GetStatistics();

        /// <summary>
        ///     Removes excess inactive objects from the pool. Default implementation returns 0.
        ///     Override in derived pools to implement actual trimming.
        /// </summary>
        /// <param name="maxRetained">Maximum number of inactive objects to retain.</param>
        /// <returns>The number of objects removed.</returns>
        public virtual int Trim(int maxRetained) {
            return 0;
        }

        /// <summary>
        ///     Internal typed get logic implemented by derived pools.
        /// </summary>
        /// <returns>An object from the pool.</returns>
        protected abstract object OnGetInternal();

        /// <summary>
        ///     Internal typed return logic implemented by derived pools.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        protected abstract void OnReturnInternal(object obj);
    }

    public class ObjectPool<TClass> : ObjectPool, IObjectPool<TClass> where TClass : class, new()
    {
        private static readonly object _instanceLockObject = new object();

        private static ObjectPool<TClass> _shared;

        private readonly object _lockObject = new object();
        private readonly ObjectPoolOptions<TClass> _options;
        private HashSet<TClass> _inactiveLookup;
        private Stack<TClass> _objects;
        private ObjectPoolStatistics _statistics;

        /// <summary>
        ///     Creates a pool with default options.
        /// </summary>
        public ObjectPool() : this(null) { }

        /// <summary>
        ///     Creates a pool with the specified options.
        /// </summary>
        /// <param name="options">Pool configuration options, or <c>null</c> for defaults.</param>
        public ObjectPool(ObjectPoolOptions<TClass> options) {
            _options = options ?? new ObjectPoolOptions<TClass>();
        }

        /// <summary>
        ///     Gets the lazily-initialized shared singleton instance.
        /// </summary>
        public static ObjectPool<TClass> Shared {
            get {
                if (null == _shared) {
                    lock (_instanceLockObject) {
                        if (null == _shared) {
                            var instance = new ObjectPool<TClass>();
                            instance.Create();
                            _shared = instance;
                        }
                    }
                }

                return _shared;
            }
        }

        /// <summary>
        ///     Returns an object to the pool, applying destroy and overflow policies.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj" /> is null.</exception>
        public void Return(TClass obj) {
            ThrowHelper.ThrowIfNull(obj, nameof(obj));

            // Returning an item always ends its current use cycle before retention policy is applied.
            if (obj is IDestroyable destroyable) {
                destroyable.Destroy();
            }

            _options.OnReturn?.Invoke(obj);

            if (obj is IPoolObjectResetable resetable) {
                resetable.Reset();
            }

            if (obj is Object pooledObject && pooledObject.Destroyed) {
                _statistics.TotalReturnCount++;
                _statistics.TotalDestroyedCount++;
                _statistics.CountAll--;
                _options.OnDestroy?.Invoke(obj);
                return;
            }

            lock (_lockObject) {
                if (_inactiveLookup.Contains(obj)) {
                    _statistics.TotalDuplicateReturnCount++;
                    return;
                }

                _statistics.TotalReturnCount++;

                if (_objects.Count >= _options.MaxSize &&
                    _options.OverflowPolicy == ObjectPoolOverflowPolicy.DestroyReturned) {
                    _statistics.TotalDestroyedCount++;
                    _statistics.CountAll--;
                    _options.OnDestroy?.Invoke(obj);
                    return;
                }

                _objects.Push(obj);
                _inactiveLookup.Add(obj);
            }
        }

        /// <summary>
        ///     Gets an object from the pool, creating a new instance if none is available.
        /// </summary>
        /// <returns>A pooled or newly created instance of <typeparamref name="TClass" />.</returns>
        public new TClass Get() {
            TClass item;
            lock (_lockObject) {
                if (_objects.Count > 0) {
                    item = _objects.Pop();
                    _inactiveLookup.Remove(item);
                }
                else {
                    item = new TClass();
                    _statistics.CountAll++;
                    _statistics.TotalCreatedCount++;
                }

                _statistics.TotalGetCount++;
            }

            _options.OnGet?.Invoke(item);
            return item;
        }

        /// <summary>
        ///     Removes excess inactive objects from the pool, releasing them for garbage collection.
        /// </summary>
        /// <param name="maxRetained">
        ///     Maximum number of inactive objects to retain. If the pool holds more, the excess are
        ///     discarded.
        /// </param>
        /// <returns>The number of objects removed.</returns>
        public int Trim(int maxRetained) {
            var removed = 0;
            lock (_lockObject) {
                while (_objects.Count > maxRetained) {
                    var item = _objects.Pop();
                    _inactiveLookup.Remove(item);
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
        public override ObjectPoolStatistics GetStatistics() {
            lock (_lockObject) {
                _statistics.CountInactive = _objects?.Count ?? 0;
                _statistics.CountActive = _statistics.CountAll - _statistics.CountInactive;
                return _statistics;
            }
        }

        /// <summary>
        ///     Initializes the pool storage and pre-populates with <see cref="ObjectPoolOptions{TClass}.InitialCapacity" />
        ///     instances.
        /// </summary>
        protected override void OnCreate() {
            lock (_lockObject) {
                _objects = new Stack<TClass>(_options.InitialCapacity);
                _inactiveLookup = new HashSet<TClass>();
                _statistics = default;
                for (var i = 0; i < _options.InitialCapacity; i++) {
                    var item = new TClass();
                    _objects.Push(item);
                    _inactiveLookup.Add(item);
                    _statistics.CountAll++;
                    _statistics.TotalCreatedCount++;
                }
            }
        }

        /// <summary>
        ///     Clears and releases all pooled instances.
        /// </summary>
        protected override void OnDestroy() {
            lock (_lockObject) {
                _objects?.Clear();
                _inactiveLookup?.Clear();
                _objects = null;
                _inactiveLookup = null;
            }
        }

        /// <summary>
        ///     Delegates to the typed <see cref="Get" /> method.
        /// </summary>
        /// <returns>An object from the pool.</returns>
        protected override object OnGetInternal() {
            return Get();
        }

        /// <summary>
        ///     Validates type and delegates to the typed <see cref="Return(TClass)" /> method.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj" /> is null.</exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when <paramref name="obj" /> type does not match
        ///     <typeparamref name="TClass" />.
        /// </exception>
        protected override void OnReturnInternal(object obj) {
            ThrowHelper.ThrowIfNull(obj, nameof(obj));
            ThrowHelper.ThrowIfTypeMismatch(obj.GetType(), typeof(TClass));
            Return(obj as TClass);
        }
    }

    public class ObjectPool<TClass, TAllocator> : ObjectPool, IObjectPool<TClass>
        where TClass : class, new()
        where TAllocator : IPoolObjectAllocator<TClass>, new()
    {
        private static readonly object _instanceLockObject = new object();

        private static ObjectPool<TClass, TAllocator> _shared;

        private readonly object _lockObject = new object();
        private readonly ObjectPoolOptions<TClass> _options;
        private TAllocator _allocator;
        private HashSet<TClass> _inactiveLookup;
        private Stack<TClass> _objects;
        private ObjectPoolStatistics _statistics;

        /// <summary>
        ///     Creates a pool with default options.
        /// </summary>
        public ObjectPool() : this(null) { }

        /// <summary>
        ///     Creates a pool with the specified options.
        /// </summary>
        /// <param name="options">Pool configuration options, or <c>null</c> for defaults.</param>
        public ObjectPool(ObjectPoolOptions<TClass> options) {
            _options = options ?? new ObjectPoolOptions<TClass>();
        }

        /// <summary>
        ///     Gets the lazily-initialized shared singleton instance.
        /// </summary>
        public static ObjectPool<TClass, TAllocator> Shared {
            get {
                if (null == _shared) {
                    lock (_instanceLockObject) {
                        if (null == _shared) {
                            var instance = new ObjectPool<TClass, TAllocator>();
                            instance.Create();
                            _shared = instance;
                        }
                    }
                }

                return _shared;
            }
        }

        /// <summary>
        ///     Gets an object from the pool, allocating via <typeparamref name="TAllocator" /> if none is available.
        /// </summary>
        /// <returns>A pooled or newly allocated instance of <typeparamref name="TClass" />.</returns>
        public new TClass Get() {
            TClass item;
            lock (_lockObject) {
                if (_objects.Count > 0) {
                    item = _objects.Pop();
                    _inactiveLookup.Remove(item);
                }
                else {
                    item = _allocator.Alloc();
                    _statistics.CountAll++;
                    _statistics.TotalCreatedCount++;
                }

                _statistics.TotalGetCount++;
            }

            _options.OnGet?.Invoke(item);
            return item;
        }

        /// <summary>
        ///     Returns an object to the pool, applying reset, destroy, and overflow policies.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj" /> is null.</exception>
        public void Return(TClass obj) {
            ThrowHelper.ThrowIfNull(obj, nameof(obj));

            // Returning an item always ends its current use cycle before retention policy is applied.
            if (obj is IDestroyable destroyable) {
                destroyable.Destroy();
            }

            _options.OnReturn?.Invoke(obj);
            _allocator.Reset(obj);

            if (obj is IPoolObjectResetable resetable) {
                resetable.Reset();
            }

            if (obj is Object pooledObject && pooledObject.Destroyed) {
                _statistics.TotalReturnCount++;
                _statistics.TotalDestroyedCount++;
                _statistics.CountAll--;
                _options.OnDestroy?.Invoke(obj);
                return;
            }

            lock (_lockObject) {
                if (_inactiveLookup.Contains(obj)) {
                    _statistics.TotalDuplicateReturnCount++;
                    return;
                }

                _statistics.TotalReturnCount++;

                if (_objects.Count >= _options.MaxSize &&
                    _options.OverflowPolicy == ObjectPoolOverflowPolicy.DestroyReturned) {
                    _statistics.TotalDestroyedCount++;
                    _statistics.CountAll--;
                    _options.OnDestroy?.Invoke(obj);
                    return;
                }

                _objects.Push(obj);
                _inactiveLookup.Add(obj);
            }
        }

        /// <summary>
        ///     Removes excess inactive objects from the pool, releasing them for garbage collection.
        /// </summary>
        /// <param name="maxRetained">
        ///     Maximum number of inactive objects to retain. If the pool holds more, the excess are
        ///     discarded.
        /// </param>
        /// <returns>The number of objects removed.</returns>
        public int Trim(int maxRetained) {
            var removed = 0;
            lock (_lockObject) {
                while (_objects.Count > maxRetained) {
                    var item = _objects.Pop();
                    _inactiveLookup.Remove(item);
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
        public override ObjectPoolStatistics GetStatistics() {
            lock (_lockObject) {
                _statistics.CountInactive = _objects?.Count ?? 0;
                _statistics.CountActive = _statistics.CountAll - _statistics.CountInactive;
                return _statistics;
            }
        }

        /// <summary>
        ///     Initializes the allocator, pool storage, and pre-populates with instances.
        /// </summary>
        protected override void OnCreate() {
            _allocator = new TAllocator();
            lock (_lockObject) {
                _objects = new Stack<TClass>(_options.InitialCapacity);
                _inactiveLookup = new HashSet<TClass>();
                _statistics = default;
                for (var i = 0; i < _options.InitialCapacity; i++) {
                    var item = _allocator.Alloc();
                    _objects.Push(item);
                    _inactiveLookup.Add(item);
                    _statistics.CountAll++;
                    _statistics.TotalCreatedCount++;
                }
            }
        }

        /// <summary>
        ///     Clears and releases all pooled instances.
        /// </summary>
        protected override void OnDestroy() {
            lock (_lockObject) {
                _objects?.Clear();
                _inactiveLookup?.Clear();
                _objects = null;
                _inactiveLookup = null;
            }
        }

        /// <summary>
        ///     Delegates to the typed <see cref="Get" /> method.
        /// </summary>
        /// <returns>An object from the pool.</returns>
        protected override object OnGetInternal() {
            return Get();
        }

        /// <summary>
        ///     Validates type and delegates to the typed <see cref="Return(TClass)" /> method.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj" /> is null.</exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when <paramref name="obj" /> type does not match
        ///     <typeparamref name="TClass" />.
        /// </exception>
        protected override void OnReturnInternal(object obj) {
            ThrowHelper.ThrowIfNull(obj, nameof(obj));
            ThrowHelper.ThrowIfTypeMismatch(obj.GetType(), typeof(TClass));
            Return(obj as TClass);
        }
    }
}