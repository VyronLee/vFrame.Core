// ------------------------------------------------------------
//         File: IPooledObjectPolicy.cs
//        Brief: Unified policy interface for object pool creation and return operations
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-15 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    /// <summary>
    ///     Defines a policy for creating and returning objects to a pool.
    /// </summary>
    /// <typeparam name="T">The type of object being pooled (must be a reference type).</typeparam>
    public interface IPooledObjectPolicy<T> where T : class
    {
        /// <summary>
        ///     Creates a new instance of <typeparamref name="T" />.
        /// </summary>
        /// <returns>A newly created instance.</returns>
        T Create();

        /// <summary>
        ///     Determines whether the given object can be returned to the pool.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        /// <returns><c>true</c> if the object can be returned to the pool; otherwise, <c>false</c>.</returns>
        bool Return(T obj);
    }

    /// <summary>
    ///     Default pooled object policy using optional factory and return callbacks.
    /// </summary>
    /// <typeparam name="T">The type of object being pooled (must be a reference type).</typeparam>
    public sealed class DefaultPooledObjectPolicy<T> : IPooledObjectPolicy<T> where T : class
    {
        private readonly Func<T> _factory;
        private readonly Action<T> _onReturn;

        /// <summary>
        ///     Initializes a new instance with optional factory and return callbacks.
        /// </summary>
        /// <param name="factory">
        ///     Optional factory function to create instances. If <c>null</c>, uses <see cref="Activator.CreateInstance{T}()" />.
        /// </param>
        /// <param name="onReturn">
        ///     Optional action invoked when an object is returned to the pool.
        /// </param>
        public DefaultPooledObjectPolicy(Func<T> factory = null, Action<T> onReturn = null) {
            _factory = factory;
            _onReturn = onReturn;
        }

        /// <summary>
        ///     Creates a new instance using the factory if provided; otherwise uses Activator.CreateInstance.
        /// </summary>
        /// <returns>A newly created instance.</returns>
        public T Create() {
            return _factory != null ? _factory() : Activator.CreateInstance<T>();
        }

        /// <summary>
        ///     Invokes the onReturn callback (if provided) and allows the object to be pooled.
        /// </summary>
        /// <param name="obj">The object being returned.</param>
        /// <returns><c>true</c> to indicate the object can be pooled.</returns>
        public bool Return(T obj) {
            _onReturn?.Invoke(obj);
            return true;
        }
    }

    /// <summary>
    ///     Adapter that wraps an <see cref="IPoolObjectAllocator{T}" /> as an <see cref="IPooledObjectPolicy{T}" />.
    /// </summary>
    /// <typeparam name="T">The type of object being pooled.</typeparam>
    /// <typeparam name="TAllocator">The allocator type.</typeparam>
    public sealed class AllocatorPooledObjectPolicy<T, TAllocator> : IPooledObjectPolicy<T>
        where T : class, new()
        where TAllocator : IPoolObjectAllocator<T>, new()
    {
        private readonly TAllocator _allocator;

        /// <summary>
        ///     Initializes a new instance, creating the allocator lazily on first use.
        /// </summary>
        public AllocatorPooledObjectPolicy() {
            _allocator = new TAllocator();
        }

        /// <summary>
        ///     Allocates a new instance using the wrapped allocator.
        /// </summary>
        /// <returns>A newly allocated instance.</returns>
        public T Create() {
            return _allocator.Alloc();
        }

        /// <summary>
        ///     Resets the object using the wrapped allocator.
        /// </summary>
        /// <param name="obj">The object to reset.</param>
        /// <returns><c>true</c> to indicate the object can be pooled.</returns>
        public bool Return(T obj) {
            _allocator.Reset(obj);
            return true;
        }
    }
}
