//------------------------------------------------------------
//        File:  PrefabPool.cs
//       Brief:  PrefabPool
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-03-29 16:46
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System;
using System.Collections;
using vFrame.Core.Interface.SpawnPools;
using Object = UnityEngine.Object;

namespace vFrame.Core.SpawnPools.Pools
{
    public class PrefabPool<T> : PoolBase<T>, IPool<T> where T: Object
    {
        private readonly T _prefab;
        public PrefabPool(string poolName, T prefab, int lifetime)
            :base(poolName, lifetime)
        {
            _prefab = prefab;
        }

        protected override T DoSpawn()
        {
            return Object.Instantiate(_prefab);
        }

        protected override IEnumerator DoSpawnAsync(Action<T> callback)
        {
            var obj = Object.Instantiate(_prefab);
            callback(obj);
            yield break;
        }
    }
}