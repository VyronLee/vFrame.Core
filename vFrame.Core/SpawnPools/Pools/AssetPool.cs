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
using UnityEngine;
using vFrame.Core.SpawnPools.Behaviours;
using vFrame.Core.SpawnPools.Provider;
using Logger = vFrame.Core.Loggers.Logger;
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

        protected override void ObjectPreprocessBeforeReturn(Object obj) {
            base.ObjectPreprocessBeforeReturn(obj);

            var go = obj as GameObject;
            if (null == go)
                return;

            var identity = go.GetComponent<PoolObjectIdentity>();
            if (null == identity) {
                identity = go.AddComponent<PoolObjectIdentity>();
                identity.AssetPath = _assetName;
                identity.UniqueId = NewUniqueId;
#if DEBUG_SPAWNPOOLS
                Logger.Info("Pool object(id: {0}, path: {1}) created.",
                    identity.UniqueId, identity.AssetPath);
#endif
            }
            identity.IsPooling = false;
        }

        protected override bool ObjectPreprocessBeforeRecycle(Object obj) {
            var go = obj as GameObject;
            if (null == go)
                return true;

            var identity = go.GetComponent<PoolObjectIdentity>();
            if (null == identity) {
                Logger.Error("Not a valid pool object: " + obj);
                return false;
            }

            if (identity.AssetPath != _assetName) {
                Logger.Error("Object to recycle does not match the pool name, require: {0}, get: {1}",
                    _assetName, identity.AssetPath);
                return false;
            }
            identity.IsPooling = true;

            return base.ObjectPreprocessBeforeRecycle(obj);
        }

        protected override void ObjectPreprocessBeforeDestroy(Object obj) {
            var go = obj as GameObject;
            if (null == go)
                return;

            var identity = go.GetComponent<PoolObjectIdentity>();
            if (!identity)
                return;

#if DEBUG_SPAWNPOOLS
            Logger.Info("Pool object(id: {0}, path: {1}) destroyed.",
                identity.UniqueId, identity.AssetPath);
#endif

            identity.Destroyed = true;
            Object.Destroy(identity);

        }
    }
}