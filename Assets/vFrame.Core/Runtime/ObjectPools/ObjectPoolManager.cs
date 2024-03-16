using System;
using System.Collections.Generic;
using vFrame.Core.Singletons;

namespace vFrame.Core.ObjectPools
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>, IObjectPoolManager
    {
        private Dictionary<Type, IObjectPool> _pools;
        private readonly object _lockObject_1 = new object();
        private readonly object _lockObject_2 = new object();

        protected override void OnCreate() {
            lock (_lockObject_1) {
                _pools = new Dictionary<Type, IObjectPool>(256);
            }
        }

        public static ObjectPoolManager Shared => Instance();

        public T Get<T>() where T : class, new() {
            lock (_lockObject_1) {
                return GetObjectPool<T>().Get();
            }
        }
        
        public object Get(Type type) {
            lock (_lockObject_1) {
                return GetObjectPool(type).Get();
            }
        }

        public void Return<T>(T obj) where T : class, new() {
            if (obj == null) {
                throw new ArgumentNullException(nameof(obj));
            }
            lock (_lockObject_1) {
                GetObjectPool(obj.GetType()).Return(obj);
            }
        }

        public void Return(object obj) {
            if (obj == null) {
                throw new ArgumentNullException(nameof(obj));
            }
            lock (_lockObject_1) {
                GetObjectPool(obj.GetType()).Return(obj);
            }
        }

        public void TryReturn<T>(T obj) where T : class {
            if (obj == null) {
                throw new ArgumentNullException(nameof(obj));
            }
            lock (_lockObject_1) {
                if (!_pools.TryGetValue(obj.GetType(), out var pool)) {
                    return;
                }
                pool.Return(obj);
            }
        }

        public void TryReturn(object obj) {
            if (obj == null) {
                throw new ArgumentNullException(nameof(obj));
            }
            lock (_lockObject_1) {
                if (!_pools.TryGetValue(obj.GetType(), out var pool)) {
                    return;
                }
                pool.Return(obj);
            }
        }

        public IObjectPool<T> GetObjectPool<T>() where T : class, new() {
            lock (_lockObject_2) {
                if (_pools.TryGetValue(typeof(T), out var pool)) {
                    return (IObjectPool<T>) pool;
                }

                var objPool = new ObjectPool<T>();
                objPool.Initialize();
                _pools.Add(typeof(T), objPool);
                return objPool;
            }
        }

        public IObjectPool GetObjectPool(Type type) {
            lock (_lockObject_2) {
                if (_pools.TryGetValue(type, out var pool)) {
                    return pool;
                }

                var objectPoolType = typeof(ObjectPool<>).MakeGenericType(type);
                var objPool = Activator.CreateInstance(objectPoolType) as ObjectPool;
                if (null == objPool) {
                    throw new ApplicationException("Create object pool failed, type: " + type.FullName);
                }
                objPool.Initialize();
                _pools.Add(type, objPool);
                return objPool;
            }
        }

        public IObjectPool<TClass> GetObjectPool<TClass, TAllocator>()
            where TClass : class, new()
            where TAllocator : IPoolObjectAllocator<TClass>, new() {

            lock (_lockObject_2) {
                if (_pools.TryGetValue(typeof(TClass), out var pool)) {
                    return (IObjectPool<TClass>) pool;
                }

                var objPool = new ObjectPool<TClass, TAllocator>();
                objPool.Initialize();
                _pools.Add(typeof(TClass), objPool);
                return objPool;
            }
        }
    }
}