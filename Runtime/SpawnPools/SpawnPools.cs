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
using System.Linq;
using UnityEngine;
using vFrame.Core.Base;
using vFrame.Core.ObjectPools.Builtin;
using vFrame.Core.Unity.Asynchronous;
using vFrame.Core.Unity.Extensions;

namespace vFrame.Core.Unity.SpawnPools
{
    public class SpawnPools : BaseObject<IGameObjectLoaderFactory, SpawnPoolsSettings>, ISpawnPools
    {
        private const string PoolName = nameof(SpawnPools);
        private AsyncRequestCtrl _asyncRequestCtrl;
        private Comparison<string> _comparison;
        private SpawnPoolsContext _context;
        private int _lastGC;
        private IGameObjectLoaderFactory _loaderFactory;
        private GameObject _parent;

        private Dictionary<string, Pool> _pools;
        private SpawnPoolsSettings _settings;

        public void Recycle(GameObject obj) {
            ThrowIfDestroyed();

            var identity = obj.GetComponent<PoolObjectIdentity>();
            if (null == identity) {
                SpawnPoolsDebug.Warning("Not a valid pool object: " + obj.name);
                return;
            }
            GetPool(identity.AssetPath).Recycle(obj);
        }

        public IPreloadAsyncRequest PreloadAsync(string[] assetPaths) {
            ThrowIfDestroyed();

            var request = _asyncRequestCtrl.CreateRequest<PreloadAsyncRequest>();
            request.AssetPaths = assetPaths.ToList();
            request.SpawnPools = this;
            return request;
        }

        public void Update() {
            ThrowIfDestroyed();

            _asyncRequestCtrl.Update();

            if (++_lastGC < _settings.GCInterval) {
                return;
            }
            _lastGC = 0;

            var pools = ListPool<string>.Shared.Get();

            // Clear timeout pools
            foreach (var kv in _pools) {
                var pool = kv.Value;
                if (!pool.IsTimeout()) {
                    continue;
                }
                SpawnPoolsDebug.Log("Pool({0}) timeout, destroying..", kv.Key);
                pool.Clear();
                pools.Add(kv.Key);
            }
            pools.Clear();

            // Clear pools by frequency
            if (_pools.Count < _settings.Capacity) {
                ListPool<string>.Shared.Return(pools);
                return;
            }

            foreach (var kv in _pools) {
                pools.Add(kv.Key);
            }
            pools.Sort(_comparison);

            for (var i = _settings.Capacity; i < pools.Count; i++) {
                SpawnPoolsDebug.Log("Pool({0}) over capacity, destroying..", pools[i]);
                _pools[pools[i]].Clear();
            }
            ListPool<string>.Shared.Return(pools);
        }

        public GameObject Spawn(string assetPath, Transform parent = null) {
            ThrowIfDestroyed();
            return GetPool(assetPath).Spawn(parent);
        }

        public ILoadAsyncRequest SpawnAsync(string assetPath, Transform parent = null) {
            ThrowIfDestroyed();

            var request = GetPool(assetPath).SpawnAsync(parent);
            _asyncRequestCtrl.AddRequest(request);
            return request;
        }

        private IPool GetPool(string assetPath) {
            if (_pools.TryGetValue(assetPath, out var pool)) {
                return pool;
            }
            pool = CreatePool(assetPath);
            _pools[assetPath] = pool;
            _parent.name = $"{PoolName}({_parent.transform.childCount + 1})";
            return pool;
        }

        private Pool CreatePool(string assetPath) {
            if (!(_loaderFactory.CreateLoader(assetPath) is IGameObjectLoader builder)) {
                return null;
            }
            var pool = new Pool();
            pool.Create(assetPath, _context, builder);
            return pool;
        }

        protected override void OnCreate(IGameObjectLoaderFactory factory, SpawnPoolsSettings settings) {
            _loaderFactory = factory ?? new DefaultGameObjectLoaderFactory();
            _comparison = CompareBySpawnedTimes;
            _pools = new Dictionary<string, Pool>();
            _asyncRequestCtrl = AsyncRequestCtrl.Create();
            _settings = settings;
            _parent = new GameObject(PoolName).DontDestroyEx();
            _parent.transform.position = _settings.RootPosition;

            _context = new SpawnPoolsContext {
                Settings = _settings,
                Parent = _parent.transform
            };
        }

        protected override void OnDestroy() {
            Clear();

            _asyncRequestCtrl?.Destroy();
            _asyncRequestCtrl = null;

            if (_parent) {
                _parent.DestroyEx();
            }
            _parent = null;

            _settings = null;
            _pools = null;
            _loaderFactory = null;
            _context = null;

            SpawnPoolsDebug.Log("Spawn pools destroyed.");
        }

        public void Clear() {
            ThrowIfDestroyed();
            foreach (var kv in _pools) {
                kv.Value.Destroy();
            }
            _pools.Clear();
        }

        private int CompareBySpawnedTimes(string poolNameA, string poolNameB) {
            return _pools[poolNameB].SpawnedTimes.CompareTo(_pools[poolNameA].SpawnedTimes);
        }
    }
}