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
using vFrame.Core.SpawnPools.Builders;
using Logger = vFrame.Core.Loggers.Logger;

namespace vFrame.Core.SpawnPools
{
    public class SpawnPools : BaseObject<IGameObjectBuilderFactory, int, int>, ISpawnPools
    {
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

        private readonly Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();

        private IGameObjectBuilderFactory _builderFromPathFactory;
        private IGameObjectBuilderFactory _builderFromPrefabInstanceFactory;

        private int _lifetime;
        private int _capacity;

        private int _lastGC;

        protected override void OnCreate(IGameObjectBuilderFactory factory, int lifetime, int capacity) {
            _builderFromPathFactory = factory ?? new DefaultGameObjectBuilderFromPathFactory();
            _builderFromPrefabInstanceFactory = new DefaultGameObjectBuilderFromPrefabInstanceFactory();
            _lifetime = lifetime;
            _capacity = capacity;
        }

        protected override void OnDestroy() {
            Clear();

#if DEBUG_SPAWNPOOLS
            Logger.Info("Spawn pools destroyed.");
#endif
        }

        public void Clear() {
            foreach (var kv in _pools)
                kv.Value.Clear();
            _pools.Clear();
        }

        public IPool this[string assetName] {
            get {
                if (_pools.ContainsKey(assetName))
                    return _pools[assetName];

                if (!(_builderFromPathFactory.CreateBuilder() is IGameObjectBuilderFromPath builder))
                    return null;

                builder.Create(assetName);

                var pool = new Pool();
                pool.Create(assetName, _lifetime, builder);

                _pools.Add(assetName, pool);
                PoolsParent.name = $"Pools({PoolsParent.transform.childCount})";

                return pool;
            }
        }

        public IPool this[GameObject prefab] {
            get {
                var prefabCode = $"Prefab-{prefab.name}-{prefab.GetInstanceID()}";
                if (_pools.ContainsKey(prefabCode))
                    return _pools[prefabCode];

                if (!(_builderFromPrefabInstanceFactory.CreateBuilder() is IGameObjectBuilderFromPrefabInstance builder))
                    return null;

                builder.Create(prefab);

                var pool = new Pool();
                pool.Create(prefabCode, _lifetime, builder);

                _pools.Add(prefabCode, pool);
                PoolsParent.name = $"Pools({PoolsParent.transform.childCount})";

                return pool;
            }
        }

        public void Update() {
            if (++_lastGC < SpawnPoolsSetting.GCInterval)
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