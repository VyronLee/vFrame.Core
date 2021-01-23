namespace vFrame.Core.ObjectPools
{
    public interface IObjectPoolManager
    {
        T Get<T>() where T : class, new();

        void Return<T>(T obj) where T : class, new();

        void TryReturn<T>(T obj) where T : class;

        IObjectPool<T> GetObjectPool<T>() where T : class, new();

        IObjectPool<TClass> GetObjectPool<TClass, TAllocator>()
            where TClass : class, new()
            where TAllocator : IPoolObjectAllocator<TClass>, new();
    }
}