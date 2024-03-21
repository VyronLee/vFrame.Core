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
using vFrame.Core.Generic;
using vFrame.Core.Unity.Extensions;

namespace vFrame.Core.Unity.SpawnPools
{
    internal class Pool : BaseObject<string, SpawnPoolsContext, IGameObjectLoader>, IPool
    {
        private readonly Queue<GameObject> _objects = new Queue<GameObject>();
        private IGameObjectLoader _builder;
        private SpawnPoolsContext _context;
        private int _lastTime;
        private GameObject _poolGo;
        private string _poolName;
        private int _uniqueId;
        private int NewUniqueId => ++_uniqueId;

        internal int SpawnedTimes { get; private set; }

        public GameObject Spawn(Transform parent) {
            if (!parent) {
                parent = _context.Parent;
            }
            var obj = TryGetFromPool();
            if (!obj) {
                SpawnPoolsDebug.Log("No objects in pool({0}), spawning new one..", _poolName);
                obj = _builder.Load();
            }
            obj.transform.SetParent(parent, false);
            OnSpawned(obj);
            return obj;
        }

        public ILoadAsyncRequest SpawnAsync(Transform parent) {
            if (!parent) {
                parent = _context.Parent;
            }
            var obj = TryGetFromPool();
            if (!obj) {
                SpawnPoolsDebug.Log("No objects in pool({0}), spawning new one..", _poolName);
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

            var callback = AsyncRequestFinishedCallback.CreateWithSharedPools();
            callback.Pool = this;
            callback.Parent = parent;
            callback.Register(request);

            return request;
        }

        public void Recycle(GameObject obj) {
            if (null == obj) {
                SpawnPoolsDebug.Error("Object to recycle cannot be null!");
                return;
            }

            SpawnPoolsDebug.Log("Recycling object into pool({0})", obj.name);

            if (!OnReturn(obj)) {
                obj.DestroyEx();
                return;
            }
            _objects.Enqueue(obj);
        }

        public int Count => _objects.Count;

        protected override void OnCreate(string poolName, SpawnPoolsContext context, IGameObjectLoader builder) {
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
                SpawnPoolsDebug.Log("Spawning object from pool({0}) ", _poolName);
                var obj = _objects.Dequeue();
                if (null != obj) {
                    return obj;
                }
                SpawnPoolsDebug.Warning("Spawned object is NULL , DON'T destroy managed object outside the pool!");
            }
            return null;
        }

        private void OnSpawned(GameObject obj) {
            SpawnedTimes += 1;
            _lastTime = Time.frameCount;

            if (!obj) {
                SpawnPoolsDebug.Warning("Get gameObject callback, but target == null, pool name: " + _poolName);
                return;
            }

            var identity = obj.GetComponent<PoolObjectIdentity>();
            if (null == identity) {
                identity = obj.AddComponent<PoolObjectIdentity>();
                identity.AssetPath = _poolName;
                identity.UniqueId = NewUniqueId;
                SpawnPoolsDebug.Log("Pool object(id: {0}, path: {1}) created.", identity.UniqueId, identity.AssetPath);
            }
            identity.IsPooling = false;
        }

        private bool OnReturn(GameObject go) {
            var identity = go.GetComponent<PoolObjectIdentity>();
            if (null == identity) {
                SpawnPoolsDebug.Warning("Not a valid pool object: " + go);
                return false;
            }
            if (identity.AssetPath != _poolName) {
                SpawnPoolsDebug.Warning("Object to recycle does not match the pool name, require: {0}, get: {1}",
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

            SpawnPoolsDebug.Log("Spawn pool cleared: {0}", _poolName);
        }

        private class AsyncRequestFinishedCallback : ActionCallback<AsyncRequestFinishedCallback>
        {
            public Transform Parent { get; set; }
            public Pool Pool { get; set; }
            private LoadAsyncRequest Request { get; set; }

            public void Register(LoadAsyncRequest request) {
                Request = request;
                Request.OnFinish += Callback;
            }

            protected override void OnCallback() {
                var go = Request.GameObject;
                go.transform.SetParent(Parent);
                Pool.OnSpawned(go);
            }

            protected override void OnDestroy() {
                Request.OnFinish -= Callback;
                Request = null;
                Parent = null;
                Pool = null;
                base.OnDestroy();
            }
        }
    }
}