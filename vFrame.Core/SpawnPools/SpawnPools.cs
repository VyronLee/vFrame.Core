//------------------------------------------------------------
//        File:  SpawnPools.cs
//       Brief:  Spawn pools.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-09-08 23:44
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System.Collections.Generic;
using UnityEngine;
using vFrame.Core.ObjectPools.Builtin;
using vFrame.Core.SpawnPools.Pools;
using Logger = vFrame.Core.Loggers.Logger;

namespace vFrame.Core.SpawnPools
{
    public class SpawnPools<T> : ISpawnPools<T> where T : Object
    {
        private const int DefaultCapacity = 40;
        private const int DefaultLifeTime = 30 * 60 * 5; // 5min by 30fps
        private const int GCInterval = 600; // 600 frames, 20s by 30fps

        private static GameObject _poolsParent;

        public static GameObject PoolsParent {
            get {
                if (!_poolsParent) {
                    _poolsParent = new GameObject("Pools");
                    _poolsParent.transform.position = SpawnPoolsSetting.RootPosition;
                    Object.DontDestroyOnLoad(PoolsParent);
                }

                return _poolsParent;
            }
        }

        private readonly Dictionary<string, PoolBase<T>> _pools = new Dictionary<string, PoolBase<T>>();

        private IAssetsProvider _provider;
        private IAssetsProviderAsync _providerAsync;
        private int _lifetime;
        private int _capacity;

        private int _lastGC;

        public ISpawnPools<T> Initialize(IAssetsProvider provider, IAssetsProviderAsync providerAsync = null,
            int lifetime = DefaultLifeTime, int capacity = DefaultCapacity) {
            _provider = provider;
            _providerAsync = providerAsync;
            _lifetime = lifetime;
            _capacity = capacity;

            return this;
        }

        public void Clear() {
            foreach (var kv in _pools)
                kv.Value.Clear();
            _pools.Clear();
        }

        public void Destroy() {
            Clear();

#if DEBUG_SPAWNPOOLS
            Logger.Info("Spawn pools destroyed: {0}", typeof(T).Name);
#endif
        }

        public IPool<T> this[string assetName] {
            get {
                if (_pools.ContainsKey(assetName))
                    return _pools[assetName] as IPool<T>;

                _pools.Add(assetName, new AssetPool<T>(assetName, _provider, _providerAsync, _lifetime));
                PoolsParent.name = string.Format("Pools({0})", PoolsParent.transform.childCount);
                return _pools[assetName] as IPool<T>;
            }
        }

        public IPool<T> this[T prefab] {
            get {
                var prefabCode = string.Format("Prefab-{0}-{1}", prefab.name, prefab.GetInstanceID());
                if (_pools.ContainsKey(prefabCode))
                    return _pools[prefabCode] as IPool<T>;

                _pools.Add(prefabCode, new PrefabPool<T>(prefabCode, prefab, _lifetime));
                PoolsParent.name = string.Format("Pools({0})", PoolsParent.transform.childCount);
                return _pools[prefabCode] as IPool<T>;
            }
        }

        public void Update() {
            if (++_lastGC < GCInterval)
                return;
            _lastGC = 0;

            var pools = ListPool<string>.Get();

            // Clear overtime pools
            foreach (var kv in _pools) {
                var pool = kv.Value;
                if (!pool.IsTimeout())
                    continue;
#if DEBUG_SPAWNPOOLS
                Debug.LogFormat("Pool({0}) timeout, destroying..", kv.Key);
#endif
                pool.Clear();
                pools.Add(kv.Key);
            }

            pools.Clear();

            // Clear pools by frequency
            if (_pools.Count < _capacity)
                return;

            foreach (var kv in _pools)
                pools.Add(kv.Key);
            pools.Sort((a, b) => _pools[b].SpawnedTimes.CompareTo(_pools[a].SpawnedTimes));

            for (var i = _capacity; i < pools.Count; i++) {
#if DEBUG_SPAWNPOOLS
                Debug.LogFormat("Pool({0}) over capacity, destroying..", pools[i]);
#endif
                _pools[pools[i]].Clear();
            }

            ListPool<string>.Return(pools);
        }
    }
}