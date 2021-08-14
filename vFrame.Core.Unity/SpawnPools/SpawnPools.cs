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
using vFrame.Core.Base;
using vFrame.Core.ObjectPools.Builtin;
using vFrame.Core.SpawnPools.Loaders;

namespace vFrame.Core.SpawnPools
{
    public class SpawnPools : BaseObject<IGameObjectLoaderFactory, SpawnPoolsSetting>, ISpawnPools
    {
        private GameObject _poolsParent;
        private SpawnPoolsSetting _poolsSetting;

        public GameObject PoolsParent {
            get {
                if (!_poolsParent) {
                    _poolsParent = new GameObject("Pools");
                    _poolsParent.transform.position = _poolsSetting.RootPosition;
                    Object.DontDestroyOnLoad(PoolsParent);
                }

                return _poolsParent;
            }
        }

        private readonly Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();

        private IGameObjectLoaderFactory _builderFromPathFactory;

        private int _lastGC;

        public SpawnPoolsSetting PoolsSetting => _poolsSetting;

        protected override void OnCreate(IGameObjectLoaderFactory factory, SpawnPoolsSetting poolsSetting) {
            _builderFromPathFactory = factory ?? new DefaultGameObjectLoaderFromPathFactory();
            _poolsSetting = poolsSetting;
        }

        protected override void OnDestroy() {
            Clear();

#if DEBUG_SPAWNPOOLS
            Logger.Info("Spawn pools destroyed.");
#endif
        }

        public void Clear() {
            foreach (var kv in _pools) {
                kv.Value.Clear();
                kv.Value.Destroy();
            }
            _pools.Clear();
        }

        public IPool this[string assetName] {
            get {
                if (_pools.ContainsKey(assetName))
                    return _pools[assetName];

                if (!(_builderFromPathFactory.CreateLoader() is IGameObjectLoaderFromPath builder))
                    return null;

                builder.Create(assetName);

                var pool = new Pool();
                pool.Create(assetName, this, builder);

                _pools.Add(assetName, pool);
                PoolsParent.name = $"Pools({PoolsParent.transform.childCount})";

                return pool;
            }
        }

        public void Update() {
            if (++_lastGC < _poolsSetting.GCInterval)
                return;
            _lastGC = 0;

            var pools = ListPool<string>.Shared.Get();

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
            if (_pools.Count < _poolsSetting.Capacity)
                return;

            foreach (var kv in _pools)
                pools.Add(kv.Key);
            pools.Sort((a, b) => _pools[b].SpawnedTimes.CompareTo(_pools[a].SpawnedTimes));

            for (var i = _poolsSetting.Capacity; i < pools.Count; i++) {
#if DEBUG_SPAWNPOOLS
                Debug.LogFormat("Pool({0}) over capacity, destroying..", pools[i]);
#endif
                _pools[pools[i]].Clear();
            }

            ListPool<string>.Shared.Return(pools);
        }
    }
}