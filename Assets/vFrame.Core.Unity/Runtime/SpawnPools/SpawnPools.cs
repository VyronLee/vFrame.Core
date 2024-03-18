//------------------------------------------------------------
//        File:  SpawnPools.cs
//       Brief:  Spawn pools.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-08 23:44
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using vFrame.Core.Base;
using vFrame.Core.ObjectPools.Builtin;
using vFrame.Core.Unity.Coroutine;
using Debug = vFrame.Core.Unity.SpawnPools.SpawnPoolDebug;
using Object = UnityEngine.Object;

namespace vFrame.Core.Unity.SpawnPools
{
    public class SpawnPools : BaseObject<IGameObjectLoaderFactory, SpawnPoolsSetting>, ISpawnPools
    {
        private const string PoolName = nameof(SpawnPools);

        private GameObject _poolsParent;
        private SpawnPoolsSetting _poolsSetting;
        private CoroutinePool _coroutinePool;

        internal GameObject PoolsParent {
            get {
                if (_poolsParent)
                    return _poolsParent;

                _poolsParent = new GameObject(PoolName);
                _poolsParent.transform.position = _poolsSetting.RootPosition;
                Object.DontDestroyOnLoad(PoolsParent);
                return _poolsParent;
            }
        }

        private Comparison<string> _comparison;
        private readonly Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();
        private IGameObjectLoaderFactory _builderFromPathFactory;
        private int _lastGC;

        internal SpawnPoolsSetting PoolsSetting => _poolsSetting;
        internal CoroutinePool CoroutinePool => _coroutinePool;

        protected override void OnCreate(IGameObjectLoaderFactory factory, SpawnPoolsSetting poolsSetting) {
            _builderFromPathFactory = factory ?? new DefaultGameObjectLoaderFromPathFactory();
            _poolsSetting = poolsSetting;
            _coroutinePool = new CoroutinePool(nameof(SpawnPools), _poolsSetting.AsyncLoadCount);
            _comparison = CompareBySpawnedTimes;
        }

        protected override void OnDestroy() {
            Clear();

            _coroutinePool?.Destroy();
            _coroutinePool = null;

            if (_poolsParent) {
                Object.Destroy(_poolsParent);
            }
            _poolsParent = null;

            Debug.Log("Spawn pools destroyed.");
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
                if (_pools.TryGetValue(assetName, out var pool)) {
                    return pool;
                }

                if (!(_builderFromPathFactory.CreateLoader() is IGameObjectLoaderFromPath builder)) {
                    return null;
                }
                builder.Create(assetName);

                pool = new Pool();
                pool.Create(assetName, this, builder);

                _pools.Add(assetName, pool);
                PoolsParent.name = $"{PoolName}({PoolsParent.transform.childCount})";

                return pool;
            }
        }

        public IPreloadAsyncRequest PreloadAsync(string[] assetPaths) {
            var request = new PreloadAsyncRequest();
            request.Create(this, assetPaths);
            request.Setup(_coroutinePool);
            return request;
        }

        public void Update() {
            if (++_lastGC < _poolsSetting.GCInterval) {
                return;
            }
            _lastGC = 0;

            var pools = ListPool<string>.Shared.Get();

            // Clear timeout pools
            foreach (var kv in _pools) {
                var pool = kv.Value;
                if (!pool.IsTimeout())
                    continue;
                Debug.Log("Pool({0}) timeout, destroying..", kv.Key);
                pool.Clear();
                pools.Add(kv.Key);
            }
            pools.Clear();

            // Clear pools by frequency
            if (_pools.Count < _poolsSetting.Capacity) {
                ListPool<string>.Shared.Return(pools);
                return;
            }

            foreach (var kv in _pools) {
                pools.Add(kv.Key);
            }
            pools.Sort(_comparison);

            for (var i = _poolsSetting.Capacity; i < pools.Count; i++) {
                Debug.Log("Pool({0}) over capacity, destroying..", pools[i]);
                _pools[pools[i]].Clear();
            }
            ListPool<string>.Shared.Return(pools);
        }

        private int CompareBySpawnedTimes(string poolNameA, string poolNameB) {
            return _pools[poolNameB].SpawnedTimes.CompareTo(_pools[poolNameA].SpawnedTimes);
        }
    }
}