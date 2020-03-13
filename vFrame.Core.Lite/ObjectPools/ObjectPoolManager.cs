using System;
using System.Collections.Generic;
using vFrame.Core.Singletons;

namespace vFrame.Core.ObjectPools
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        private Dictionary<Type, IObjectPool> _pools;
        private readonly object _lockObject = new object();

        protected override void OnCreate() {
            lock (_lockObject) {
                _pools = new Dictionary<Type, IObjectPool>(256);
            }
        }

        public void RegisterPool<T>(IObjectPool<T> pool) {
            lock (_lockObject) {
                _pools.Add(typeof(T), pool);
            }
        }

        public T Get<T>() where T : class, new() {
            IObjectPool pool = null;
            lock (_lockObject) {
                if (_pools.TryGetValue(typeof(T), out pool)) return ((IObjectPool<T>) pool).GetObject();
            }

            return ObjectPool<T>.Get();
        }

        public void Return<T>(T obj) {
            IObjectPool pool = null;
            lock (_lockObject) {
                if (_pools.TryGetValue(obj.GetType(), out pool)) {
                    pool.ReturnObject(obj);
                    return;
                }
            }

            throw new ArgumentOutOfRangeException("obj", "No object pool of type: " + obj.GetType().Name);
        }
    }
}