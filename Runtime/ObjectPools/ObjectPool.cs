//------------------------------------------------------------
//        File:  ObjectPool.cs
//       Brief:  ObjectPool
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-07-09 19:09
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System;
using System.Collections.Generic;
using vFrame.Core.Loggers;

namespace vFrame.Core.ObjectPools
{
    public abstract class ObjectPool : IObjectPool
    {
        protected readonly LogTag LogTag = new LogTag("ObjectPool");

        internal void Initialize() {
            OnInitialize();
        }

        protected abstract void OnInitialize();
        protected abstract object OnGetInternal();
        protected abstract void OnReturnInternal(object obj);
        
        public object Get() {
            return OnGetInternal();
        }

        public void Return(object obj) {
            OnReturnInternal(obj);
        }
    }

    public class ObjectPool<TClass> : ObjectPool, IObjectPool<TClass> where TClass : class, new()
    {
        private const int InitSize = 128;
        private Stack<TClass> _objects;

        private readonly object _lockObject = new object();
        private static readonly object _instanceLockObject = new object();

        private static ObjectPool<TClass> _shared;

        public static ObjectPool<TClass> Shared {
            get {
                if (null == _shared)
                    lock (_instanceLockObject) {
                        if (null == _shared) {
                            var instance = new ObjectPool<TClass>();
                            instance.Initialize();
                            _shared = instance;
                        }
                    }

                return _shared;
            }
        }

        protected override void OnInitialize() {
            lock (_lockObject) {
                _objects = new Stack<TClass>(InitSize);
                for (var i = 0; i < InitSize; i++) {
                    _objects.Push(new TClass());
                }
            }
        }

        protected override object OnGetInternal() {
            return Get();
        }

        protected override void OnReturnInternal(object obj) {
            if (obj == null) {
                throw new ArgumentNullException(nameof(obj));
            }
            if (obj.GetType() != typeof(TClass)) {
                throw new ArgumentException($"object type mismatch, required: {typeof(TClass).FullName}, got: {obj.GetType().FullName}");
            }
            Return(obj as TClass);
        }

        public void Return(TClass obj) {
            if (null == obj) {
                Logger.Error(LogTag, "Return object cannot be null.");
                return;
            }

            lock (_lockObject) {
                if (_objects.Contains(obj))
                    return;
                if (obj is IPoolObjectResetable resetable) {
                    resetable.Reset();
                }
                _objects.Push(obj);
            }
        }

        public new TClass Get() {
            lock (_lockObject) {
                return _objects.Count > 0 ? _objects.Pop() : new TClass();
            }
        }
    }

    public class ObjectPool<TClass, TAllocator> : ObjectPool, IObjectPool<TClass>
        where TClass : class, new()
        where TAllocator : IPoolObjectAllocator<TClass>, new()
    {
        private const int InitSize = 128;
        private Stack<TClass> _objects;
        private TAllocator _allocator;

        private readonly object _lockObject = new object();
        private static readonly object _instanceLockObject = new object();

        private static ObjectPool<TClass, TAllocator> _shared;

        public static ObjectPool<TClass, TAllocator> Shared {
            get {
                if (null == _shared)
                    lock (_instanceLockObject) {
                        if (null == _shared) {
                            var instance = new ObjectPool<TClass, TAllocator>();
                            instance.Initialize();
                            _shared = instance;
                        }
                    }

                return _shared;
            }
        }

        protected override void OnInitialize() {
            _allocator = new TAllocator();
            lock (_lockObject) {
                _objects = new Stack<TClass>(InitSize);
                for (var i = 0; i < InitSize; i++) {
                    _objects.Push(_allocator.Alloc());
                }
            }
        }

        protected override object OnGetInternal() {
            return Get();
        }

        protected override void OnReturnInternal(object obj) {
            if (obj == null) {
                throw new ArgumentNullException(nameof(obj));
            }
            if (obj.GetType() != typeof(TClass)) {
                throw new ArgumentException($"object type mismatch, required: {typeof(TClass).FullName}, got: {obj.GetType().FullName}");
            }
            Return(obj as TClass);
        }

        public new TClass Get() {
            lock (_lockObject) {
                return _objects.Count > 0 ? _objects.Pop() : _allocator.Alloc();
            }
        }

        public void Return(TClass obj) {
            if (null == obj) {
                Logger.Error(LogTag, "Return object cannot be null.");
                return;
            }

            _allocator.Reset(obj);

            lock (_lockObject) {
                if (_objects.Contains(obj))
                    return;
                _objects.Push(obj);
            }
        }
    }
}