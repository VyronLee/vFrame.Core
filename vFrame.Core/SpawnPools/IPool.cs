using System;
using System.Collections;
using Object = UnityEngine.Object;

namespace vFrame.Core.Interface.SpawnPools
{
    public interface IPool<T> where T: Object
    {
        T Spawn();
        IEnumerator SpawnAsync(Action<Object> callback);
        void Recycle(T obj);
        int Count { get; }
    }
}