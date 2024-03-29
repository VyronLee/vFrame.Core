using System;

namespace vFrame.Core.ObjectPools
{
    public interface IObjectPoolManager
    {
        T Get<T>() where T : class, new();
        object Get(Type type);

        void Return<T>(T obj) where T : class, new();
        void Return(object obj);

        void TryReturn<T>(T obj) where T : class;
        void TryReturn(object obj);

        IObjectPool<T> GetObjectPool<T>() where T : class, new();
        IObjectPool GetObjectPool(Type type);

        IObjectPool<TClass> GetObjectPool<TClass, TAllocator>()
            where TClass : class, new()
            where TAllocator : IPoolObjectAllocator<TClass>, new();
    }
}