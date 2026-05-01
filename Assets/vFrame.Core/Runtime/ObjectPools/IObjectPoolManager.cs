// ------------------------------------------------------------
//         File: IObjectPoolManager.cs
//        Brief: Object pool manager interface providing unified get, return, and pool query capabilities
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
    public interface IObjectPoolManager
    {
        /// <summary>
        ///     Gets an object from the pool registered for <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of object to get.</typeparam>
        /// <returns>A pooled instance of <typeparamref name="T" />.</returns>
        T Get<T>() where T : class, new();

        /// <summary>
        ///     Gets an object from the pool registered for the specified <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type of object to get.</param>
        /// <returns>A pooled instance of the specified type.</returns>
        object Get(Type type);

        /// <summary>
        ///     Returns an object to the pool registered for <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="obj">The object to return.</param>
        void Return<T>(T obj) where T : class, new();

        /// <summary>
        ///     Returns an object to the pool registered for its runtime type.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        void Return(object obj);

        /// <summary>
        ///     Attempts to return an object to the pool; no-op if no pool is registered for its type.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="obj">The object to return.</param>
        void TryReturn<T>(T obj) where T : class;

        /// <summary>
        ///     Attempts to return an object to the pool; no-op if no pool is registered for its runtime type.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        void TryReturn(object obj);

        /// <summary>
        ///     Gets the typed object pool registered for <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The pooled object type.</typeparam>
        /// <returns>The <see cref="IObjectPool{T}" /> instance.</returns>
        IObjectPool<T> GetObjectPool<T>() where T : class, new();

        /// <summary>
        ///     Gets the non-generic object pool registered for the specified <paramref name="type" />.
        /// </summary>
        /// <param name="type">The pooled object type.</param>
        /// <returns>The <see cref="IObjectPool" /> instance.</returns>
        IObjectPool GetObjectPool(Type type);

        /// <summary>
        ///     Gets or creates a typed object pool using the specified allocator type.
        /// </summary>
        /// <typeparam name="TClass">The pooled object type.</typeparam>
        /// <typeparam name="TAllocator">The allocator type used to create and reset instances.</typeparam>
        /// <returns>The <see cref="IObjectPool{TClass}" /> instance using <typeparamref name="TAllocator" />.</returns>
        IObjectPool<TClass> GetObjectPool<TClass, TAllocator>()
            where TClass : class, new()
            where TAllocator : IPoolObjectAllocator<TClass>, new();

        /// <summary>
        ///     Gets the existing pool for <typeparamref name="T" /> without creating one.
        /// </summary>
        /// <typeparam name="T">The pooled object type.</typeparam>
        /// <param name="pool">The existing pool, or <c>null</c> if none is registered.</param>
        /// <returns><c>true</c> if a pool exists; otherwise <c>false</c>.</returns>
        bool TryGetObjectPool<T>(out IObjectPool<T> pool) where T : class, new();

        /// <summary>
        ///     Gets the existing pool for the specified <paramref name="type" /> without creating one.
        /// </summary>
        /// <param name="type">The pooled object type.</param>
        /// <param name="pool">The existing pool, or <c>null</c> if none is registered.</param>
        /// <returns><c>true</c> if a pool exists; otherwise <c>false</c>.</returns>
        bool TryGetObjectPool(Type type, out IObjectPool pool);

        /// <summary>
        ///     Removes excess inactive objects from all registered pools.
        /// </summary>
        /// <param name="maxRetainedPerPool">Maximum inactive objects to retain per pool.</param>
        /// <returns>Total number of objects removed across all pools.</returns>
        int TrimAll(int maxRetainedPerPool);

        /// <summary>
        ///     Gets the number of registered pools.
        /// </summary>
        int GetPoolCount();

        /// <summary>
        ///     Registers a custom pool for type <typeparamref name="T" />.
        ///     Replaces any existing registration.
        /// </summary>
        void Register<T>(IObjectPool<T> pool) where T : class;

        /// <summary>
        ///     Registers a custom pool for the specified type.
        /// </summary>
        void Register(Type type, IObjectPool pool);

        /// <summary>
        ///     Unregisters the pool for <typeparamref name="T" />.
        /// </summary>
        /// <returns>true if a pool was removed; false if none was registered.</returns>
        bool Unregister<T>() where T : class;

        /// <summary>
        ///     Unregisters the pool for the specified type.
        /// </summary>
        bool Unregister(Type type);

        /// <summary>
        ///     Gets all registered pools with their associated types.
        /// </summary>
        IEnumerable<(Type Type, IObjectPool Pool)> GetAllPools();
    }
}