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

namespace vFrame.Core
{
    public interface IObjectPoolManager
    {
        /// <summary>
        /// Gets an object from the pool registered for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to get.</typeparam>
        /// <returns>A pooled instance of <typeparamref name="T"/>.</returns>
        T Get<T>() where T : class, new();

        /// <summary>
        /// Gets an object from the pool registered for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of object to get.</param>
        /// <returns>A pooled instance of the specified type.</returns>
        object Get(Type type);

        /// <summary>
        /// Returns an object to the pool registered for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="obj">The object to return.</param>
        void Return<T>(T obj) where T : class, new();

        /// <summary>
        /// Returns an object to the pool registered for its runtime type.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        void Return(object obj);

        /// <summary>
        /// Attempts to return an object to the pool; no-op if no pool is registered for its type.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="obj">The object to return.</param>
        void TryReturn<T>(T obj) where T : class;

        /// <summary>
        /// Attempts to return an object to the pool; no-op if no pool is registered for its runtime type.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        void TryReturn(object obj);

        /// <summary>
        /// Gets the typed object pool registered for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The pooled object type.</typeparam>
        /// <returns>The <see cref="IObjectPool{T}"/> instance.</returns>
        IObjectPool<T> GetObjectPool<T>() where T : class, new();

        /// <summary>
        /// Gets the non-generic object pool registered for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The pooled object type.</param>
        /// <returns>The <see cref="IObjectPool"/> instance.</returns>
        IObjectPool GetObjectPool(Type type);

        /// <summary>
        /// Gets or creates a typed object pool using the specified allocator type.
        /// </summary>
        /// <typeparam name="TClass">The pooled object type.</typeparam>
        /// <typeparam name="TAllocator">The allocator type used to create and reset instances.</typeparam>
        /// <returns>The <see cref="IObjectPool{TClass}"/> instance using <typeparamref name="TAllocator"/>.</returns>
        IObjectPool<TClass> GetObjectPool<TClass, TAllocator>()
            where TClass : class, new()
            where TAllocator : IPoolObjectAllocator<TClass>, new();
    }
}
