// ------------------------------------------------------------
//         File: PooledObject.cs
//        Brief: Auto-return wrapper struct for pooled objects, supporting the using pattern
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-15 19:09:00
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    /// <summary>
    ///     Auto-return wrapper for pooled objects. Enables using-pattern disposal to automatically
    ///     return items to the pool when leaving scope.
    /// </summary>
    /// <typeparam name="T">The pooled object type (must be a reference type).</typeparam>
    public readonly struct PooledObject<T> : IDisposable where T : class
    {
        private readonly IObjectPool<T> _pool;
        private readonly T _item;

        /// <summary>
        ///     Internal constructor used by object pools to create a pooled wrapper.
        /// </summary>
        /// <param name="pool">The pool that owns this item.</param>
        /// <param name="item">The pooled object instance.</param>
        internal PooledObject(IObjectPool<T> pool, T item)
        {
            _pool = pool;
            _item = item;
        }

        /// <summary>
        ///     Gets the pooled object instance.
        /// </summary>
        public T Item => _item;

        /// <summary>
        ///     Returns the pooled object to its pool. Safe to call on default struct instances.
        /// </summary>
        public void Dispose()
        {
            _pool?.Return(_item);
        }
    }
}
