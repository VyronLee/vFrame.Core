//------------------------------------------------------------
//        File:  ObjectPool.cs
//       Brief:  ObjectPool
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-07-09 19:09
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Collections.Generic;
using vFrame.Core.Base;
using vFrame.Core.Exceptions;
using vFrame.Core.Loggers;

namespace vFrame.Core.ObjectPools
{
    public abstract class ObjectPool : BaseObject, IObjectPool
    {
        protected readonly LogTag LogTag = new LogTag("ObjectPool");

        /// <summary>
        /// Starts a new pooled use cycle.
        /// </summary>
        public object Get() {
            return OnGetInternal();
        }

        /// <summary>
        /// Ends the current pooled use cycle and applies pool-managed return policy.
        /// </summary>
        public void Return(object obj) {
            OnReturnInternal(obj);
        }

        public abstract ObjectPoolStatistics GetStatistics();

        protected abstract object OnGetInternal();
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

        public ObjectPool() : this(null) { }

        public ObjectPool(ObjectPoolOptions<TClass> options) {
            _options = options ?? new ObjectPoolOptions<TClass>();
        }

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
                _options.OnDestroy?.Invoke(obj);
                return;
            }

            lock (_lockObject) {
                if (_inactiveLookup.Contains(obj)) {
                    _statistics.TotalDuplicateReturnCount++;
                    return;
                }

                _statistics.TotalReturnCount++;

                if (_objects.Count >= _options.MaxSize && _options.OverflowPolicy == ObjectPoolOverflowPolicy.DestroyReturned) {
                    _statistics.TotalDestroyedCount++;
                    _options.OnDestroy?.Invoke(obj);
                    return;
                }

                _objects.Push(obj);
                _inactiveLookup.Add(obj);
            }
        }

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

        protected override void OnDestroy() {
            lock (_lockObject) {
                _objects?.Clear();
                _inactiveLookup?.Clear();
                _objects = null;
                _inactiveLookup = null;
            }
        }

        public override ObjectPoolStatistics GetStatistics() {
            lock (_lockObject) {
                _statistics.CountInactive = _objects?.Count ?? 0;
                _statistics.CountActive = _statistics.CountAll - _statistics.CountInactive;
                return _statistics;
            }
        }

        protected override object OnGetInternal() {
            return Get();
        }

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

        public ObjectPool() : this(null) { }

        public ObjectPool(ObjectPoolOptions<TClass> options) {
            _options = options ?? new ObjectPoolOptions<TClass>();
        }

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
                _options.OnDestroy?.Invoke(obj);
                return;
            }

            lock (_lockObject) {
                if (_inactiveLookup.Contains(obj)) {
                    _statistics.TotalDuplicateReturnCount++;
                    return;
                }

                _statistics.TotalReturnCount++;

                if (_objects.Count >= _options.MaxSize && _options.OverflowPolicy == ObjectPoolOverflowPolicy.DestroyReturned) {
                    _statistics.TotalDestroyedCount++;
                    _options.OnDestroy?.Invoke(obj);
                    return;
                }

                _objects.Push(obj);
                _inactiveLookup.Add(obj);
            }
        }

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

        protected override void OnDestroy() {
            lock (_lockObject) {
                _objects?.Clear();
                _inactiveLookup?.Clear();
                _objects = null;
                _inactiveLookup = null;
            }
        }

        public override ObjectPoolStatistics GetStatistics() {
            lock (_lockObject) {
                _statistics.CountInactive = _objects?.Count ?? 0;
                _statistics.CountActive = _statistics.CountAll - _statistics.CountInactive;
                return _statistics;
            }
        }

        protected override object OnGetInternal() {
            return Get();
        }

        protected override void OnReturnInternal(object obj) {
            ThrowHelper.ThrowIfNull(obj, nameof(obj));
            ThrowHelper.ThrowIfTypeMismatch(obj.GetType(), typeof(TClass));
            Return(obj as TClass);
        }
    }
}
