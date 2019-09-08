using UnityEngine;

namespace vFrame.Core.Interface.SpawnPools
{
    public interface ISpawnPools<T> where T: Object
    {
        ISpawnPools<T> Initialize(IAssetsProvider provider, IAssetsProviderAsync providerAsync, int lifetime, int capacity);
        IPool<T> this[string assetName] { get; }
        IPool<T> this[T prefab] { get; }
        void Update();
    }
}