//------------------------------------------------------------
//        File:  ObjectPool.cs
//       Brief:  ObjectPool
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-07-09 19:09
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

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

        public abstract T GetObject<T>() where T : class;

        public abstract void ReturnObject<T>(T obj);
    }

    public class ObjectPool<TClass> : ObjectPool, IObjectPool<TClass> where TClass : class, new()
    {
        private const int InitSize = 128;
        private Stack<TClass> _objects;

        private readonly object _lockObject = new object();
        private static readonly object _instanceLockObject = new object();

        private static ObjectPool<TClass> _instance;

        private static ObjectPool<TClass> Instance {
            get {
                if (null == _instance)
                    lock (_instanceLockObject) {
                        if (null == _instance) {
                            _instance = new ObjectPool<TClass>();
                            _instance.Initialize();
                        }
                    }

                return _instance;
            }
        }

        protected override void OnInitialize() {
            lock (_lockObject) {
                _objects = new Stack<TClass>(InitSize);
                for (var i = 0; i < InitSize; i++) _objects.Push(new TClass());
            }

            _instance = this;

            ObjectPoolManager.Instance().RegisterPool(this);
        }

        public void ReturnObject(TClass obj) {
            if (null == obj) {
                Logger.Error(LogTag, "Return object cannot be null.");
                return;
            }

            lock (_lockObject) {
                if (_objects.Contains(obj))
                    return;
                if (obj is IPoolObjectResetable resetable) resetable.Reset();
                _objects.Push(obj);
            }
        }

        public TClass GetObject() {
            lock (_lockObject) {
                return _objects.Count > 0 ? _objects.Pop() : new TClass();
            }
        }

        public override T GetObject<T>() {
            return GetObject() as T;
        }

        public override void ReturnObject<T>(T obj) {
            Return(obj as TClass);
        }

        public static TClass Get() {
            return Instance.GetObject();
        }

        public static void Return(TClass obj) {
            Instance.ReturnObject(obj);
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

        private static ObjectPool<TClass, TAllocator> _instance;

        private static ObjectPool<TClass, TAllocator> Instance {
            get {
                if (null == _instance)
                    lock (_instanceLockObject) {
                        if (null == _instance) {
                            _instance = new ObjectPool<TClass, TAllocator>();
                            _instance.Initialize();
                        }
                    }

                return _instance;
            }
        }

        protected override void OnInitialize() {
            _allocator = new TAllocator();
            lock (_lockObject) {
                _objects = new Stack<TClass>(InitSize);
                for (var i = 0; i < InitSize; i++) _objects.Push(_allocator.Alloc());
            }

            _instance = this;

            ObjectPoolManager.Instance().RegisterPool(this);
        }

        public TClass GetObject() {
            lock (_lockObject) {
                return _objects.Count > 0 ? _objects.Pop() : _allocator.Alloc();
            }
        }

        public void ReturnObject(TClass obj) {
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

        public override T GetObject<T>() {
            return GetObject() as T;
        }

        public override void ReturnObject<T>(T obj) {
            Return(obj as TClass);
        }

        public static TClass Get() {
            return Instance.GetObject();
        }

        public static void Return(TClass obj) {
            Instance.ReturnObject(obj);
        }
    }
}