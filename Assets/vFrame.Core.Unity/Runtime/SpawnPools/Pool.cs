// ------------------------------------------------------------
//         File: Pool.cs
//        Brief: Retained Unity instance pool for a single asset
//                path; manages instantiated object reuse with
//                spawn, recycle, and async loading support.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-03-29 16:53:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using vFrame.Core;
using vFrame.Core.Unity;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Retained Unity instance pool for one asset path. This class only manages instantiated
    /// object reuse and does not own resource discovery, download, or patch semantics.
    /// </summary>
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

        /// <summary>
        /// Gets the total number of times this pool has spawned an instance.
        /// </summary>
        internal int SpawnedTimes { get; private set; }

        /// <summary>
        /// Starts or resumes an instance use cycle for this asset path.
        /// </summary>
        /// <param name="parent">Optional parent transform; defaults to the pool root.</param>
        /// <returns>A <see cref="GameObject"/> ready for use.</returns>
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

        /// <summary>
        /// Starts an async instance acquire flow that still resolves through the same pooled
        /// identity and spawn bookkeeping as the synchronous path.
        /// </summary>
        /// <param name="parent">Optional parent transform; defaults to the pool root.</param>
        /// <returns>An <see cref="ILoadAsyncRequest"/> that delivers the object on completion.</returns>
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

        /// <summary>
        /// Ends the current use cycle for an instance and returns it to this pool when valid.
        /// Invalid or mismatched objects are destroyed instead of being retained.
        /// </summary>
        /// <param name="obj">The object to recycle back into the pool.</param>
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

        /// <summary>
        /// Gets the number of inactive objects currently retained in this pool.
        /// </summary>
        public int Count => _objects.Count;

        /// <summary>
        /// Initializes the pool with the given name, shared context, and object loader.
        /// </summary>
        /// <param name="poolName">Asset path used as the pool identifier.</param>
        /// <param name="context">Shared spawn pools context providing settings and parenting.</param>
        /// <param name="builder">Loader responsible for creating new instances on demand.</param>
        protected override void OnCreate(string poolName, SpawnPoolsContext context, IGameObjectLoader builder) {
            _context = context;
            _lastTime = Time.frameCount;
            _poolName = poolName;
            _builder = builder;

            _poolGo = new GameObject($"Pool({poolName})");
            _poolGo.transform.SetParent(_context.Parent.transform, false);
        }

        /// <summary>
        /// Destroys all retained objects and the pool's container GameObject.
        /// </summary>
        protected override void OnDestroy() {
            Clear();

            if (_poolGo) {
                _poolGo.DestroyEx();
            }
            _poolGo = null;
        }

        /// <summary>
        /// Attempts to dequeue a valid object from the inactive queue.
        /// Skips entries that have been destroyed externally.
        /// </summary>
        /// <returns>A reusable <see cref="GameObject"/>, or null when the queue is empty.</returns>
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

        /// <summary>
        /// Bookkeeping called when an object is spawned: increments counters and assigns
        /// or updates the pool object identity component.
        /// </summary>
        /// <param name="obj">The object that was just spawned.</param>
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

        /// <summary>
        /// Validates that the object belongs to this pool before accepting it back.
        /// </summary>
        /// <param name="go">The object being returned.</param>
        /// <returns><c>true</c> if the object is valid for this pool; otherwise <c>false</c>.</returns>
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

        /// <summary>
        /// Determines whether this pool has exceeded the configured inactive lifetime.
        /// </summary>
        /// <returns><c>true</c> if the pool should be evicted; otherwise <c>false</c>.</returns>
        internal bool IsTimeout() {
            return Time.frameCount - _lastTime > _context.Settings.LifeTime;
        }

        /// <summary>
        /// Destroys all retained inactive instances for this asset path.
        /// </summary>
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

        /// <summary>
        /// Callback that applies parenting and spawn bookkeeping once an async load completes.
        /// </summary>
        private class AsyncRequestFinishedCallback : ActionCallback<AsyncRequestFinishedCallback>
        {
            public Transform Parent { get; set; }
            public Pool Pool { get; set; }
            private LoadAsyncRequest Request { get; set; }

            /// <summary>
            /// Registers this callback on the request's <c>OnFinish</c> event.
            /// </summary>
            /// <param name="request">The async request to observe.</param>
            public void Register(LoadAsyncRequest request) {
                Request = request;
                Request.OnFinish += Callback;
            }

            /// <summary>
            /// Invoked when the async request finishes; parents and marks the object as spawned.
            /// </summary>
            protected override void OnCallback() {
                var go = Request.GameObject;
                go.transform.SetParent(Parent);
                Pool.OnSpawned(go);
            }

            /// <summary>
            /// Unsubscribes from the request event and clears references.
            /// </summary>
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
