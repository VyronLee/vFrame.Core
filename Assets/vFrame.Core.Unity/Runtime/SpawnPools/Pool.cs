//------------------------------------------------------------
//        File:  PoolBase.cs
//       Brief:  PoolBase
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-03-29 16:53
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Collections.Generic;
using UnityEngine;
using vFrame.Core.Base;
using vFrame.Core.Unity.Extensions;
using Debug = vFrame.Core.Unity.SpawnPools.SpawnPoolDebug;

namespace vFrame.Core.Unity.SpawnPools
{
    internal class Pool : BaseObject<string, SpawnPoolContext, IGameObjectLoader>, IPool
    {
        private readonly Queue<GameObject> _objects = new Queue<GameObject>();
        private IGameObjectLoader _builder;
        private SpawnPoolContext _context;
        private int _lastTime;
        private GameObject _poolGo;
        private string _poolName;
        private int _uniqueId;
        private int NewUniqueId => ++_uniqueId;

        internal int SpawnedTimes { get; private set; }

        public GameObject Spawn(Transform parent) {
            var obj = TryGetFromPool();
            if (!obj) {
                Debug.Log("No objects in pool({0}), spawning new one..", _poolName);
                obj = _builder.Load();
            }
            obj.transform.SetParent(parent);
            OnSpawned(obj);
            return obj;
        }

        public ILoadAsyncRequest SpawnAsync(Transform parent) {
            var obj = TryGetFromPool();
            if (!obj) {
                Debug.Log("No objects in pool({0}), spawning new one..", _poolName);
            }

            LoadAsyncRequest request;
            if (null != obj) {
                request = new LoadAsyncRequestOnLoaded();
                request.Create();
                request.GameObject = obj;
            }
            else {
                request = _builder.LoadAsync();
                request.Create();
            }
            request.OnFinish += () => {
                request.GameObject.transform.SetParent(parent);
                OnSpawned(request.GameObject);
            };
            return request;
        }

        public void Recycle(GameObject obj) {
            if (null == obj) {
                Debug.Error("Object to recycle cannot be null!");
                return;
            }

            Debug.Log("Recycling object into pool({0})", obj.name);

            if (!OnReturn(obj)) {
                obj.DestroyEx();
                return;
            }
            _objects.Enqueue(obj);
        }

        public int Count => _objects.Count;

        protected override void OnCreate(string poolName, SpawnPoolContext context, IGameObjectLoader builder) {
            _context = context;
            _lastTime = Time.frameCount;
            _poolName = poolName;
            _builder = builder;

            _poolGo = new GameObject($"Pool({poolName})");
            _poolGo.transform.SetParent(_context.Parent.transform, false);
        }

        protected override void OnDestroy() {
            Clear();

            if (_poolGo) {
                _poolGo.DestroyEx();
            }
            _poolGo = null;
        }

        private GameObject TryGetFromPool() {
            while (_objects.Count > 0) {
                Debug.Log("Spawning object from pool({0}) ", _poolName);
                var obj = _objects.Dequeue();
                if (null != obj) {
                    return obj;
                }
                Debug.Warning("Spawned object is NULL , DON'T destroy managed object outside the pool!");
            }
            return null;
        }

        private void OnSpawned(GameObject obj) {
            SpawnedTimes += 1;
            _lastTime = Time.frameCount;

            if (!obj) {
                Debug.Warning("Get gameObject callback, but target == null, pool name: " + _poolName);
                return;
            }

            var identity = obj.GetComponent<PoolObjectIdentity>();
            if (null == identity) {
                identity = obj.AddComponent<PoolObjectIdentity>();
                identity.AssetPath = _poolName;
                identity.UniqueId = NewUniqueId;
                Debug.Log("Pool object(id: {0}, path: {1}) created.", identity.UniqueId, identity.AssetPath);
            }
            identity.IsPooling = false;
        }

        private bool OnReturn(GameObject go) {
            var identity = go.GetComponent<PoolObjectIdentity>();
            if (null == identity) {
                Debug.Warning("Not a valid pool object: " + go);
                return false;
            }
            if (identity.AssetPath != _poolName) {
                Debug.Warning("Object to recycle does not match the pool name, require: {0}, get: {1}",
                    _poolName, identity.AssetPath);
                return false;
            }
            identity.IsPooling = true;
            return true;
        }

        internal bool IsTimeout() {
            return Time.frameCount - _lastTime > _context.Settings.LifeTime;
        }

        internal void Clear() {
            foreach (var obj in _objects) {
                if (!obj) {
                    continue;
                }
                obj.DestroyEx();
            }
            _objects.Clear();

            Debug.Log("Spawn pool cleared: {0}", _poolName);
        }
    }
}