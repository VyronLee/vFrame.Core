namespace vFrame.Core.ObjectPools
{
    public interface IObjectPool
    {
        void Return(object obj);
        object Get();
    }

    public interface IObjectPool<T> : IObjectPool
    {
        /// <summary>
        /// Get instance from pool.
        /// </summary>
        /// <returns></returns>
        new T Get();

        /// <summary>
        /// Return instance to pool.
        /// </summary>
        /// <param name="obj"></param>
        void Return(T obj);
    }
}