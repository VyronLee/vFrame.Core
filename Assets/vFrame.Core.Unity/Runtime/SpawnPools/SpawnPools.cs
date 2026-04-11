// ------------------------------------------------------------
//         File: SpawnPools.cs
//        Brief: Retained Unity-side instance reuse layer for
//                prefab and GameObject pooling with spawn, recycle,
//                preload, and async/update lifecycle management.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 23:44:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using vFrame.Core;
using vFrame.Core.Unity;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Retained Unity-side instance reuse layer for prefab and <see cref="GameObject"/> pooling.
    /// It aligns spawn, recycle, preload, and async/update flows with the core lifecycle and
    /// pooling model without taking on resource-runtime responsibilities.
    /// </summary>
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

        /// <summary>
        /// Ends the current usage cycle for a pooled object and returns it to its matching pool.
        /// </summary>
        /// <param name="obj">The spawned object to recycle.</param>
        public void Recycle(GameObject obj) {
            ThrowIfDestroyed();

            var identity = obj.GetComponent<PoolObjectIdentity>();
            if (null == identity) {
                SpawnPoolsDebug.Warning("Not a valid pool object: " + obj.name);
                return;
            }
            GetPool(identity.AssetPath).Recycle(obj);
        }

        /// <summary>
        /// Starts pool warm-up for the requested asset paths. This prepares retained instance reuse
        /// behavior only and does not shift SpawnPools into resource ownership.
        /// </summary>
        /// <param name="assetPaths">Array of asset paths to preload into pools.</param>
        /// <returns>An <see cref="IPreloadAsyncRequest"/> that completes when preloading is done.</returns>
        public IPreloadAsyncRequest PreloadAsync(string[] assetPaths) {
            ThrowIfDestroyed();

            var request = _asyncRequestCtrl.CreateRequest<PreloadAsyncRequest>();
            request.AssetPaths = assetPaths.ToList();
            request.SpawnPools = this;
            return request;
        }

        /// <summary>
        /// Advances async request completion and performs lightweight pool cleanup based on the
        /// configured retained-capacity and lifetime policy.
        /// </summary>
        public void Update() {
            ThrowIfDestroyed();

            _asyncRequestCtrl.Update();

            if (++_lastGC < _settings.GCInterval) {
                return;
            }
            _lastGC = 0;

            var pools = ListPool<string>.Shared.Get();

            // Expire inactive pools that have exceeded the retained lifetime window.
            foreach (var kv in _pools) {
                var pool = kv.Value;
                if (!pool.IsTimeout()) {
                    continue;
                }
                SpawnPoolsDebug.Log("Pool({0}) timeout, destroying..", kv.Key);
                pool.Clear();
                pools.Add(kv.Key);
            }
            // Remove expired pools from the dictionary to prevent unbounded growth
            foreach (var key in pools) {
                _pools.Remove(key);
            }
            pools.Clear();

            // Trim least-used pools when retained pool count exceeds configured capacity.
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
                _pools.Remove(pools[i]);
            }
            ListPool<string>.Shared.Return(pools);
        }

        /// <summary>
        /// Gets a pooled or newly loaded instance for immediate use.
        /// </summary>
        /// <param name="assetPath">Asset path identifying the prefab to spawn.</param>
        /// <param name="parent">Optional parent transform; defaults to the pool root.</param>
        /// <returns>A spawned <see cref="GameObject"/> ready for use.</returns>
        public GameObject Spawn(string assetPath, Transform parent = null) {
            ThrowIfDestroyed();
            return GetPool(assetPath).Spawn(parent);
        }

        /// <summary>
        /// Starts an async instance acquire flow. Completion is driven by <see cref="Update"/>
        /// and still resolves through the retained pooling model.
        /// </summary>
        /// <param name="assetPath">Asset path identifying the prefab to spawn.</param>
        /// <param name="parent">Optional parent transform; defaults to the pool root.</param>
        /// <returns>An <see cref="ILoadAsyncRequest"/> that delivers the spawned object on completion.</returns>
        public ILoadAsyncRequest SpawnAsync(string assetPath, Transform parent = null) {
            ThrowIfDestroyed();

            var request = GetPool(assetPath).SpawnAsync(parent);
            _asyncRequestCtrl.AddRequest(request);
            return request;
        }

        /// <summary>
        /// Retrieves an existing pool for the asset path or creates a new one.
        /// </summary>
        /// <param name="assetPath">Asset path used as the pool key.</param>
        /// <returns>The <see cref="IPool"/> managing instances for the given path.</returns>
        private IPool GetPool(string assetPath) {
            if (_pools.TryGetValue(assetPath, out var pool)) {
                return pool;
            }
            pool = CreatePool(assetPath);
            _pools[assetPath] = pool;
            _parent.name = $"{PoolName}({_parent.transform.childCount + 1})";
            return pool;
        }

        /// <summary>
        /// Creates a new <see cref="Pool"/> bound to the given asset path using the configured loader factory.
        /// </summary>
        /// <param name="assetPath">Asset path the pool will manage.</param>
        /// <returns>A newly created <see cref="Pool"/>, or null if the loader cannot be created.</returns>
        private Pool CreatePool(string assetPath) {
            if (!(_loaderFactory.CreateLoader(assetPath) is IGameObjectLoader builder)) {
                return null;
            }
            var pool = new Pool();
            pool.Create(assetPath, _context, builder);
            return pool;
        }

        /// <summary>
        /// Initializes the spawn pools runtime with the given loader factory and settings.
        /// </summary>
        /// <param name="factory">Factory used to create per-asset loaders; falls back to a default when null.</param>
        /// <param name="settings">Configuration governing capacity, lifetime, and GC behavior.</param>
        protected override void OnCreate(IGameObjectLoaderFactory factory, SpawnPoolsSettings settings) {
            _loaderFactory = factory ?? new DefaultGameObjectLoaderFactory();
            _comparison = CompareBySpawnedTimes;
            _pools = new Dictionary<string, Pool>();
            _asyncRequestCtrl = new AsyncRequestCtrl();
            _asyncRequestCtrl.Create();
            _settings = settings;
            SpawnPoolsDebug.Configure(_settings);
            _parent = new GameObject(PoolName).DontDestroyEx();
            _parent.transform.position = _settings.RootPosition;

            _context = new SpawnPoolsContext {
                Settings = _settings,
                Parent = _parent.transform
            };
        }

        /// <summary>
        /// Tears down all pools, async controllers, and the root container.
        /// </summary>
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
            SpawnPoolsDebug.Reset();
        }

        /// <summary>
        /// Destroys all tracked pools and retained inactive instances owned by this runtime.
        /// </summary>
        public void Clear() {
            ThrowIfDestroyed();
            foreach (var kv in _pools) {
                kv.Value.Destroy();
            }
            _pools.Clear();
        }

        /// <summary>
        /// Comparison delegate that orders pool names by descending spawn count for eviction.
        /// </summary>
        /// <param name="poolNameA">First pool name.</param>
        /// <param name="poolNameB">Second pool name.</param>
        /// <returns>A negative, zero, or positive value as per standard comparison semantics.</returns>
        private int CompareBySpawnedTimes(string poolNameA, string poolNameB) {
            return _pools[poolNameB].SpawnedTimes.CompareTo(_pools[poolNameA].SpawnedTimes);
        }
    }
}
