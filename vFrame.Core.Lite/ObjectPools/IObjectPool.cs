namespace vFrame.Core.ObjectPools
{
    public interface IObjectPool
    {
        /// <summary>
        /// Get instance from pool.
        /// </summary>
        /// <returns></returns>
        T GetObject<T>() where T : class;

        /// <summary>
        /// Return instance to pool.
        /// </summary>
        /// <param name="obj"></param>
        void ReturnObject<T>(T obj);
    }

    public interface IObjectPool<T> : IObjectPool
    {
        /// <summary>
        /// Get instance from pool.
        /// </summary>
        /// <returns></returns>
        T GetObject();

        /// <summary>
        /// Return instance to pool.
        /// </summary>
        /// <param name="obj"></param>
        void ReturnObject(T obj);
    }
}