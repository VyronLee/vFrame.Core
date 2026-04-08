namespace vFrame.Core.ObjectPools
{
    /// <summary>
    /// Lightweight retained object-pool contract. `Get()` starts a use cycle, `Return(...)`
    /// ends that use cycle, and pool policy decides whether the instance is retained, reset,
    /// reused later, or destroyed.
    /// </summary>
    public interface IObjectPool
    {
        /// <summary>
        /// Ends the current use cycle for an instance and lets the pool apply return policy.
        /// </summary>
        void Return(object obj);

        /// <summary>
        /// Gets an instance for a new use cycle.
        /// </summary>
        object Get();

        /// <summary>
        /// Returns observable pool statistics for diagnostics, policy verification, and capacity tracking.
        /// </summary>
        ObjectPoolStatistics GetStatistics();
    }

    public interface IObjectPool<T> : IObjectPool
    {
        /// <summary>
        ///     Gets an instance for a new use cycle.
        /// </summary>
        new T Get();

        /// <summary>
        ///     Ends the current use cycle for an instance and lets the pool apply return policy.
        /// </summary>
        void Return(T obj);
    }
}
