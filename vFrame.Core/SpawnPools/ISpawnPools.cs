//------------------------------------------------------------
//        File:  ISpawnPools.cs
//       Brief:  Spawn pools interface.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-09-08 23:47
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using UnityEngine;

namespace vFrame.Core.SpawnPools
{
    public interface ISpawnPools<T> where T : Object
    {
        ISpawnPools<T> Initialize(IAssetsProvider provider, IAssetsProviderAsync providerAsync, int lifetime,
            int capacity);

        IPool<T> this[string assetName] { get; }
        IPool<T> this[T prefab] { get; }
        void Update();
    }
}