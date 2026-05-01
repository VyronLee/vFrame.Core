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
        private readonly object _lockObject = new object();
        private Dictionary<Type, IObjectPool> _pools;

        /// <summary>
        ///     Gets the shared singleton instance of the pool manager.
        /// </summary>
        public static ObjectPoolManager Shared => Instance();

        /// <summary>
        ///     Gets an object from the pool registered for <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of object to get.</typeparam>
        /// <returns>A pooled instance of <typeparamref name="T" />.</returns>
        public T Get<T>() where T : class, new() {
            lock (_lockObject) {
                return GetObjectPool<T>().Get();
            }
        }

        /// <summary>
        ///     Gets an object from the pool registered for the specified <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type of object to get.</param>
        /// <returns>A pooled instance of the specified type.</returns>
        public object Get(Type type) {
            lock (_lockObject) {
                return GetObjectPool(type).Get();
            }
        }

        /// <summary>
        ///     Returns an object to the pool registered for <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="obj">The object to return.</param>
        public void Return<T>(T obj) where T : class, new() {
            ThrowHelper.ThrowIfNull(obj, nameof(obj));
            lock (_lockObject) {
                GetObjectPool(obj.GetType()).Return(obj);
            }
        }

        /// <summary>
        ///     Returns an object to the pool registered for its runtime type.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        public void Return(object obj) {
            ThrowHelper.ThrowIfNull(obj, nameof(obj));
            lock (_lockObject) {
                GetObjectPool(obj.GetType()).Return(obj);
            }
        }

        /// <summary>
        ///     Attempts to return an object to the pool; no-op if no pool is registered for its type.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="obj">The object to return.</param>
        public void TryReturn<T>(T obj) where T : class {
            ThrowHelper.ThrowIfNull(obj, nameof(obj));
            lock (_lockObject) {
                if (!_pools.TryGetValue(obj.GetType(), out var pool)) {
                    return;
                }

                pool.Return(obj);
            }
        }

        /// <summary>
        ///     Attempts to return an object to the pool; no-op if no pool is registered for its runtime type.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        public void TryReturn(object obj) {
            ThrowHelper.ThrowIfNull(obj, nameof(obj));
            lock (_lockObject) {
                if (!_pools.TryGetValue(obj.GetType(), out var pool)) {
                    return;
                }

                pool.Return(obj);
            }
        }

        /// <summary>
        ///     Gets or creates the typed object pool registered for <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The pooled object type.</typeparam>
        /// <returns>The <see cref="IObjectPool{T}" /> instance.</returns>
        public IObjectPool<T> GetObjectPool<T>() where T : class, new() {
            lock (_lockObject) {
                if (_pools.TryGetValue(typeof(T), out var pool)) {
                    return (IObjectPool<T>)pool;
                }

                var objPool = new ObjectPool<T>();
                _pools.Add(typeof(T), objPool);
                return objPool;
            }
        }

        /// <summary>
        ///     Gets or creates a non-generic object pool for the specified <paramref name="type" />.
        /// </summary>
        /// <param name="type">The pooled object type.</param>
        /// <returns>The <see cref="IObjectPool" /> instance.</returns>
        public IObjectPool GetObjectPool(Type type) {
            lock (_lockObject) {
                if (_pools.TryGetValue(type, out var pool)) {
                    return pool;
                }

                var objectPoolType = typeof(ObjectPool<>).MakeGenericType(type);
                var objPool = Activator.CreateInstance(objectPoolType) as IObjectPool;
                if (null == objPool) {
                    ThrowHelper.ThrowUndesiredException("Create object pool failed, type: " + type.FullName);
                    return null;
                }

                _pools.Add(type, objPool);
                return objPool;
            }
        }

        /// <summary>
        ///     Gets or creates a typed object pool using the specified allocator type.
        /// </summary>
        /// <typeparam name="TClass">The pooled object type.</typeparam>
        /// <typeparam name="TAllocator">The allocator type used to create and reset instances.</typeparam>
        /// <returns>The <see cref="IObjectPool{TClass}" /> instance using <typeparamref name="TAllocator" />.</returns>
        public IObjectPool<TClass> GetObjectPool<TClass, TAllocator>()
            where TClass : class, new()
            where TAllocator : IPoolObjectAllocator<TClass>, new() {
            lock (_lockObject) {
                if (_pools.TryGetValue(typeof(TClass), out var pool)) {
                    return (IObjectPool<TClass>)pool;
                }

                var objPool = new ObjectPool<TClass>(
                    new AllocatorPooledObjectPolicy<TClass, TAllocator>());
                _pools.Add(typeof(TClass), objPool);
                return objPool;
            }
        }

        /// <summary>
        ///     Gets the existing pool for <typeparamref name="T" /> without creating one if it does not exist.
        /// </summary>
        /// <typeparam name="T">The pooled object type.</typeparam>
        /// <param name="pool">The existing pool, or <c>null</c> if no pool is registered.</param>
        /// <returns><c>true</c> if a pool exists; otherwise <c>false</c>.</returns>
        public bool TryGetObjectPool<T>(out IObjectPool<T> pool) where T : class, new() {
            lock (_lockObject) {
                if (_pools.TryGetValue(typeof(T), out var existing)) {
                    pool = (IObjectPool<T>)existing;
                    return true;
                }

                pool = null;
                return false;
            }
        }

        /// <summary>
        ///     Gets the existing pool for the specified <paramref name="type" /> without creating one.
        /// </summary>
        /// <param name="type">The pooled object type.</param>
        /// <param name="pool">The existing pool, or <c>null</c> if no pool is registered.</param>
        /// <returns><c>true</c> if a pool exists; otherwise <c>false</c>.</returns>
        public bool TryGetObjectPool(Type type, out IObjectPool pool) {
            lock (_lockObject) {
                return _pools.TryGetValue(type, out pool);
            }
        }

        /// <summary>
        ///     Removes excess inactive objects from all registered pools.
        /// </summary>
        /// <param name="maxRetainedPerPool">Maximum number of inactive objects to retain per pool.</param>
        /// <returns>The total number of objects removed across all pools.</returns>
        public int TrimAll(int maxRetainedPerPool) {
            var totalRemoved = 0;
            lock (_lockObject) {
                foreach (var kvp in _pools) {
                    totalRemoved += kvp.Value.Trim(maxRetainedPerPool);
                }
            }

            return totalRemoved;
        }

        /// <summary>
        ///     Gets the number of registered pools.
        /// </summary>
        /// <returns>The count of registered pools.</returns>
        public int GetPoolCount() {
            lock (_lockObject) {
                return _pools.Count;
            }
        }

        public void Register<T>(IObjectPool<T> pool) where T : class {
            ThrowHelper.ThrowIfNull(pool, nameof(pool));
            lock (_lockObject) {
                _pools[typeof(T)] = pool;
            }
        }

        public void Register(Type type, IObjectPool pool) {
            ThrowHelper.ThrowIfNull(type, nameof(type));
            ThrowHelper.ThrowIfNull(pool, nameof(pool));
            lock (_lockObject) {
                _pools[type] = pool;
            }
        }

        public bool Unregister<T>() where T : class {
            lock (_lockObject) {
                return _pools.Remove(typeof(T));
            }
        }

        public bool Unregister(Type type) {
            ThrowHelper.ThrowIfNull(type, nameof(type));
            lock (_lockObject) {
                return _pools.Remove(type);
            }
        }

        public IEnumerable<(Type Type, IObjectPool Pool)> GetAllPools() {
            lock (_lockObject) {
                var result = new List<(Type, IObjectPool)>(_pools.Count);
                foreach (var kvp in _pools) {
                    result.Add((kvp.Key, kvp.Value));
                }

                return result;
            }
        }

        /// <summary>
        ///     Initializes the internal pool registry.
        /// </summary>
        protected override void OnCreate() {
            _pools = new Dictionary<Type, IObjectPool>(256);
        }

        /// <summary>
        ///     Destroys all managed pools and clears the registry.
        /// </summary>
        protected override void OnDestroy() {
            lock (_lockObject) {
                if (_pools != null) {
                    foreach (var pool in _pools.Values) {
                        if (pool is BaseObject bo) {
                            bo.Destroy();
                        }
                        else if (pool is IDisposable disposable) {
                            disposable.Dispose();
                        }
                    }

                    _pools.Clear();
                    _pools = null;
                }
            }
        }
    }
}