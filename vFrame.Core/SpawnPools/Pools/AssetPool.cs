//------------------------------------------------------------
//        File:  Pool.cs
//       Brief:  Pool
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-02-18 14:50
//   Copyright:  Copyright (c) 2018, VyronLee
//============================================================

using System;
using System.Collections;
using vFrame.Core.SpawnPools.Provider;
using Object = UnityEngine.Object;

namespace vFrame.Core.SpawnPools.Pools
{
    public class AssetPool<T> : PoolBase<T>, IPool<T> where T : Object
    {
        private readonly string _assetName;
        private readonly IAssetsProvider _provider;
        private readonly IAssetsProviderAsync _providerAsync;

        public AssetPool(string assetName, IAssetsProvider provider, IAssetsProviderAsync providerAsync, int lifetime)
            : base(assetName, lifetime) {
            _assetName = assetName;
            _provider = provider ?? new DefaultAssetsProvider();
            _providerAsync = providerAsync ?? new DefaultAssetsProviderAsync();
        }

        protected override T DoSpawn() {
            return _provider.Load(_assetName, typeof(T)) as T;
        }

        protected override IEnumerator DoSpawnAsync(Action<T> callback) {
            T obj = null;
            yield return _providerAsync.LoadAsync(_assetName, typeof(T), v => obj = v as T);

            callback(obj);
        }
    }
}