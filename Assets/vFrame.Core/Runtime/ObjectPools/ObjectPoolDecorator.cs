// ------------------------------------------------------------
//         File: ObjectPoolDecorator.cs
//        Brief: Base decorator for object pools enabling compositional behavior
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-15 19:09:00
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

namespace vFrame.Core
{
    /// <summary>
    ///     Base decorator for object pools that delegates all operations to an inner pool.
    ///     Enables compositional behavior by allowing decorators to wrap pools and override specific behaviors.
    /// </summary>
    /// <typeparam name="T">The pooled object type (must be a reference type).</typeparam>
    public abstract class ObjectPoolDecorator<T> : IObjectPool<T> where T : class
    {
        /// <summary>
        ///     The inner pool being decorated.
        /// </summary>
        protected readonly IObjectPool<T> Inner;

        /// <summary>
        ///     Creates a new decorator wrapping the specified inner pool.
        /// </summary>
        /// <param name="inner">The inner pool to decorate.</param>
        protected ObjectPoolDecorator(IObjectPool<T> inner) {
            Inner = inner;
        }

        /// <summary>
        ///     Gets an instance from the inner pool.
        /// </summary>
        /// <returns>A pooled object instance.</returns>
        public virtual T Get() => Inner.Get();

        /// <summary>
        ///     Returns an instance to the inner pool.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        public virtual void Return(T obj) => Inner.Return(obj);

        /// <summary>
        ///     Gets an instance wrapped in an auto-return disposable.
        /// </summary>
        /// <param name="item">The pooled object instance.</param>
        /// <returns>A wrapper that returns the item to the pool on disposal.</returns>
        public virtual PooledObject<T> Get(out T item) => Inner.Get(out item);

        /// <summary>
        ///     Returns statistics from the inner pool.
        /// </summary>
        /// <returns>Current pool statistics.</returns>
        public virtual ObjectPoolStatistics GetStatistics() => Inner.GetStatistics();

        /// <summary>
        ///     Removes excess inactive objects from the inner pool.
        /// </summary>
        /// <param name="maxRetained">Maximum inactive objects to retain.</param>
        /// <returns>The number of objects removed.</returns>
        public virtual int Trim(int maxRetained) => Inner.Trim(maxRetained);

        /// <summary>
        ///     Clears all inactive objects from the inner pool.
        /// </summary>
        public virtual void Clear() => Inner.Clear();

        /// <summary>
        ///     Non-generic get that delegates to the typed implementation.
        /// </summary>
        /// <returns>An object from the pool.</returns>
        object IObjectPool.Get() => Get();

        /// <summary>
        ///     Non-generic return that delegates to the typed implementation.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        void IObjectPool.Return(object obj) => Inner.Return(obj);
    }
}
