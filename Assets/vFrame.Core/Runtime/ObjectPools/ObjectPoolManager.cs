// ------------------------------------------------------------
//         File: ObjectPoolManager.cs
//        Brief: Singleton object pool manager providing typed pool registration and unified access
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-07-09 19:09:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Generic;

namespace vFrame.Core
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>, IObjectPoolManager
    {
        private readonly object _lockObject_1 = new object();
        private readonly object _lockObject_2 = new object();
        private Dictionary<Type, IObjectPool> _pools;

        /// <summary>
        /// Gets the shared singleton instance of the pool manager.
        /// </summary>
        public static ObjectPoolManager Shared => Instance();

        /// <summary>
        /// Gets an object from the pool registered for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to get.</typeparam>
        /// <returns>A pooled instance of <typeparamref name="T"/>.</returns>
        public T Get<T>() where T : class, new() {
            lock (_lockObject_1) {
                return GetObjectPool<T>().Get();
            }
        }

        /// <summary>
        /// Gets an object from the pool registered for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of object to get.</param>
        /// <returns>A pooled instance of the specified type.</returns>
        public object Get(Type type) {
            lock (_lockObject_1) {
                return GetObjectPool(type).Get();
            }
        }

        /// <summary>
        /// Returns an object to the pool registered for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="obj">The object to return.</param>
        public void Return<T>(T obj) where T : class, new() {
            ThrowHelper.ThrowIfNull(obj, nameof(obj));
            lock (_lockObject_1) {
                GetObjectPool(obj.GetType()).Return(obj);
            }
        }

        /// <summary>
        /// Returns an object to the pool registered for its runtime type.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        public void Return(object obj) {
            ThrowHelper.ThrowIfNull(obj, nameof(obj));
            lock (_lockObject_1) {
                GetObjectPool(obj.GetType()).Return(obj);
            }
        }

        /// <summary>
        /// Attempts to return an object to the pool; no-op if no pool is registered for its type.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="obj">The object to return.</param>
        public void TryReturn<T>(T obj) where T : class {
            ThrowHelper.ThrowIfNull(obj, nameof(obj));
            lock (_lockObject_1) {
                if (!_pools.TryGetValue(obj.GetType(), out var pool)) {
                    return;
                }
                pool.Return(obj);
            }
        }

        /// <summary>
        /// Attempts to return an object to the pool; no-op if no pool is registered for its runtime type.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        public void TryReturn(object obj) {
            ThrowHelper.ThrowIfNull(obj, nameof(obj));
            lock (_lockObject_1) {
                if (!_pools.TryGetValue(obj.GetType(), out var pool)) {
                    return;
                }
                pool.Return(obj);
            }
        }

        /// <summary>
        /// Gets or creates the typed object pool registered for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The pooled object type.</typeparam>
        /// <returns>The <see cref="IObjectPool{T}"/> instance.</returns>
        public IObjectPool<T> GetObjectPool<T>() where T : class, new() {
            lock (_lockObject_2) {
                if (_pools.TryGetValue(typeof(T), out var pool)) {
                    return (IObjectPool<T>)pool;
                }

                var objPool = new ObjectPool<T>();
                objPool.Create();
                _pools.Add(typeof(T), objPool);
                return objPool;
            }
        }

        /// <summary>
        /// Gets or creates a non-generic object pool for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The pooled object type.</param>
        /// <returns>The <see cref="IObjectPool"/> instance.</returns>
        public IObjectPool GetObjectPool(Type type) {
            lock (_lockObject_2) {
                if (_pools.TryGetValue(type, out var pool)) {
                    return pool;
                }

                var objectPoolType = typeof(ObjectPool<>).MakeGenericType(type);
                var objPool = Activator.CreateInstance(objectPoolType) as ObjectPool;
                if (null == objPool) {
                    ThrowHelper.ThrowUndesiredException("Create object pool failed, type: " + type.FullName);
                    return null;
                }
                objPool.Create();
                _pools.Add(type, objPool);
                return objPool;
            }
        }

        /// <summary>
        /// Gets or creates a typed object pool using the specified allocator type.
        /// </summary>
        /// <typeparam name="TClass">The pooled object type.</typeparam>
        /// <typeparam name="TAllocator">The allocator type used to create and reset instances.</typeparam>
        /// <returns>The <see cref="IObjectPool{TClass}"/> instance using <typeparamref name="TAllocator"/>.</returns>
        public IObjectPool<TClass> GetObjectPool<TClass, TAllocator>()
            where TClass : class, new()
            where TAllocator : IPoolObjectAllocator<TClass>, new() {
            lock (_lockObject_2) {
                if (_pools.TryGetValue(typeof(TClass), out var pool)) {
                    return (IObjectPool<TClass>)pool;
                }

                var objPool = new ObjectPool<TClass, TAllocator>();
                objPool.Create();
                _pools.Add(typeof(TClass), objPool);
                return objPool;
            }
        }

        /// <summary>
        /// Initializes the internal pool registry.
        /// </summary>
        protected override void OnCreate() {
            lock (_lockObject_1) {
                _pools = new Dictionary<Type, IObjectPool>(256);
            }
        }
    }
}
