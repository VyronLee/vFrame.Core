// ------------------------------------------------------------
//         File: IObjectPool.cs
//        Brief: Object pool interface supporting generic and non-generic get, return, and statistics
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-07-09 19:09:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    /// <summary>
    ///     Lightweight retained object-pool contract. `Get()` starts a use cycle, `Return(...)`
    ///     ends that use cycle, and pool policy decides whether the instance is retained, reset,
    ///     reused later, or destroyed.
    /// </summary>
    public interface IObjectPool
    {
        /// <summary>
        ///     Ends the current use cycle for an instance and lets the pool apply return policy.
        /// </summary>
        /// <param name="obj">The object to return to the pool.</param>
        void Return(object obj);

        /// <summary>
        ///     Gets an instance for a new use cycle.
        /// </summary>
        /// <returns>An object from the pool.</returns>
        object Get();

        /// <summary>
        ///     Returns observable pool statistics for diagnostics, policy verification, and capacity tracking.
        /// </summary>
        /// <returns>Current pool statistics.</returns>
        ObjectPoolStatistics GetStatistics();

        /// <summary>
        ///     Removes excess inactive objects from the pool.
        /// </summary>
        /// <param name="maxRetained">Maximum inactive objects to retain.</param>
        /// <returns>The number of objects removed.</returns>
        int Trim(int maxRetained);
    }

    public interface IObjectPool<T> : IObjectPool
    {
        /// <summary>
        ///     Gets an instance for a new use cycle.
        /// </summary>
        /// <returns>A typed object from the pool.</returns>
        new T Get();

        /// <summary>
        ///     Ends the current use cycle for an instance and lets the pool apply return policy.
        /// </summary>
        /// <param name="obj">The typed object to return to the pool.</param>
        void Return(T obj);
    }
}