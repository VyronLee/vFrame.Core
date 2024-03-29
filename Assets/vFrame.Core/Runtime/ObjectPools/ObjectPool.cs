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

        public object Get() {
            return OnGetInternal();
        }

        public void Return(object obj) {
            OnReturnInternal(obj);
        }

        protected abstract object OnGetInternal();
        protected abstract void OnReturnInternal(object obj);
    }

    public class ObjectPool<TClass> : ObjectPool, IObjectPool<TClass> where TClass : class, new()
    {
        private const int InitSize = 128;
        private static readonly object _instanceLockObject = new object();

        private static ObjectPool<TClass> _shared;

        private readonly object _lockObject = new object();
        private Stack<TClass> _objects;

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
            if (obj is IPoolObjectResetable resetable) {
                resetable.Reset();
            }
            if (obj is IDestroyable destroyable) {
                destroyable.Destroy();
            }
            lock (_lockObject) {
                if (_objects.Contains(obj)) {
                    return;
                }
                _objects.Push(obj);
            }
        }

        public new TClass Get() {
            lock (_lockObject) {
                return _objects.Count > 0 ? _objects.Pop() : new TClass();
            }
        }

        protected override void OnCreate() {
            lock (_lockObject) {
                _objects = new Stack<TClass>(InitSize);
                for (var i = 0; i < InitSize; i++) {
                    _objects.Push(new TClass());
                }
            }
        }

        protected override void OnDestroy() {
            lock (_lockObject) {
                _objects?.Clear();
                _objects = null;
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
        private const int InitSize = 128;
        private static readonly object _instanceLockObject = new object();

        private static ObjectPool<TClass, TAllocator> _shared;

        private readonly object _lockObject = new object();
        private TAllocator _allocator;
        private Stack<TClass> _objects;

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
            lock (_lockObject) {
                return _objects.Count > 0 ? _objects.Pop() : _allocator.Alloc();
            }
        }

        public void Return(TClass obj) {
            ThrowHelper.ThrowIfNull(obj, nameof(obj));

            _allocator.Reset(obj);

            if (obj is IPoolObjectResetable resetable) {
                resetable.Reset();
            }
            if (obj is IDestroyable destroyable) {
                destroyable.Destroy();
            }

            lock (_lockObject) {
                if (_objects.Contains(obj)) {
                    return;
                }
                _objects.Push(obj);
            }
        }

        protected override void OnCreate() {
            _allocator = new TAllocator();
            lock (_lockObject) {
                _objects = new Stack<TClass>(InitSize);
                for (var i = 0; i < InitSize; i++) {
                    _objects.Push(_allocator.Alloc());
                }
            }
        }

        protected override void OnDestroy() {
            lock (_lockObject) {
                _objects?.Clear();
                _objects = null;
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