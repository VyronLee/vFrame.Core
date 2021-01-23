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

        public void Return<T>(T obj) where T : class, new() {
            lock (_lockObject_1) {
                GetObjectPool<T>().Return(obj);
            }
        }

        public void TryReturn<T>(T obj) where T : class {
            lock (_lockObject_1) {
                if (!_pools.TryGetValue(typeof(T), out var pool)) {
                    return;
                }
                ((IObjectPool<T>) pool).Return(obj);
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