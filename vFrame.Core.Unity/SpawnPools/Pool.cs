//------------------------------------------------------------
//        File:  PoolBase.cs
//       Brief:  PoolBase
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-03-29 16:53
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using vFrame.Core.Base;
using vFrame.Core.Coroutine;
using vFrame.Core.Extensions.UnityEngine;
using vFrame.Core.SpawnPools.Behaviours;
using vFrame.Core.SpawnPools.Loaders;
using Logger = vFrame.Core.Loggers.Logger;
using Object = UnityEngine.Object;

namespace vFrame.Core.SpawnPools
{
    public class Pool : BaseObject<string, SpawnPools, IGameObjectLoader>, IPool
    {
        private readonly Queue<GameObject> _objects = new Queue<GameObject>();
        private int _lastTime;
        private GameObject _poolGo;

        private SpawnPools _spawnPools;
        private string _poolName;
        private IGameObjectLoader _builder;

        private int _uniqueId;
        private int NewUniqueId => ++_uniqueId;
        private Dictionary<GameObject, PoolObjectSnapshot> _snapshots;

        private Action<GameObject> _onGetGameObject;
        private Action<GameObject, IEnumerable<Type>> _onLoadCallbackImmediately;

        private CoroutinePool _coroutinePool;

        internal int SpawnedTimes { get; private set; }

        protected override void OnCreate(string poolName, SpawnPools spawnPools, IGameObjectLoader builder) {
            _spawnPools = spawnPools;
            _lastTime = Time.frameCount;
            _poolName = poolName;
            _builder = builder;
            _snapshots = new Dictionary<GameObject, PoolObjectSnapshot>(32);
            _onGetGameObject = OnGetGameObject;
            _onLoadCallbackImmediately = OnLoadCallback;

            _poolGo = new GameObject(string.Format("Pool({0})", poolName));
            _poolGo.transform.SetParent(_spawnPools.PoolsParent.transform, false);

            _coroutinePool = _spawnPools.CoroutinePool;
        }

        protected override void OnDestroy() {
            _snapshots?.Clear();
            _snapshots = null;

            if (_poolGo) {
                Object.Destroy(_poolGo);
            }
            _poolGo = null;
        }

        public void Clear() {
            foreach (var obj in _objects) {
                if (_snapshots.TryGetValue(obj, out var snapshot)) {
                    snapshot.Destroy();
                    _snapshots.Remove(obj);
                }

                if (!obj) {
                    continue;
                }
                ObjectPreprocessBeforeDestroy(obj);
                Object.Destroy(obj);
            }
            _objects.Clear();

#if DEBUG_SPAWNPOOLS
            Logger.Info("Spawn pool cleared: {0}", _poolName);
#endif
        }

        private GameObject TryGetFromPool() {
            GameObject obj = null;
            while (_objects.Count > 0) {
#if DEBUG_SPAWNPOOLS
                Debug.LogFormat("Spawning object from pool({0}) ", _poolName);
#endif
                obj = _objects.Dequeue();
                if (null == obj) {
                    Logger.Warning(
                        "Spawn object from pool, but obj == null, DONT destroy managed object outside the pool!");
                }
                else {
                    break;
                }
            }
            return obj;
        }

        public GameObject Spawn() {
            return Spawn(null);
        }

        public GameObject Spawn(IEnumerable<Type> additional) {
            var obj = TryGetFromPool();

            if (null == obj) {
#if DEBUG_SPAWNPOOLS
                Debug.LogFormat("No objects in pool({0}), spawning new one..", _poolName);
#endif
                obj = _builder.Load();
                OnLoadCallback(obj, additional);
            }

            OnGetGameObject(obj);
            return obj;
        }

        public ILoaderAsyncRequest SpawnAsync() {
            return SpawnAsync(null);
        }

        public ILoaderAsyncRequest SpawnAsync(IEnumerable<Type> additional) {
            var obj = TryGetFromPool();

#if DEBUG_SPAWNPOOLS
            Debug.LogFormat("No objects in pool({0}), spawning new one..", _poolName);
#endif

            var request = null != obj ? LoadAsyncRequestOnLoaded.Create(obj) : _builder.LoadAsync();
            request.AdditionalSnapshotTypes = additional;
            request.OnLoadCallback = _onLoadCallbackImmediately;
            request.OnGetGameObject = _onGetGameObject;
            request.Setup(_coroutinePool);
            return request;
        }

        private void OnLoadCallback(GameObject obj, IEnumerable<Type> additional) {
            if (!_snapshots.TryGetValue(obj, out var snapshot)) {
                snapshot = new PoolObjectSnapshot();
                snapshot.Create(obj, additional);
                snapshot.Take();
                _snapshots.Add(obj, snapshot);
            }
            HideGameObject(obj);
        }

        private void OnGetGameObject(GameObject obj) {
            ++SpawnedTimes;
            _lastTime = Time.frameCount;

            ObjectPreprocessBeforeReturn(obj);
        }

        public void Recycle(GameObject obj) {
            if (null == obj) {
                Logger.Error("Item to recycle cannot be null!");
                return;
            }
#if DEBUG_SPAWNPOOLS
            Debug.LogFormat("Recycling object into pool({0})", _assetName);
#endif
            if (!ObjectPreprocessBeforeRecycle(obj)) {
                // Cannot recycle, destroy it directly.
                Object.Destroy(obj);
                return;
            }

            _objects.Enqueue(obj);

            ObjectPostprocessAfterRecycle(obj);
        }

        public int Count => _objects.Count;

        public bool IsTimeout() {
            return Time.frameCount - _lastTime > _spawnPools.PoolsSetting.LifeTime;
        }

        protected virtual void ObjectPreprocessBeforeReturn(GameObject go) {
            go.transform.SetParent(null, false);

            ShowGameObject(go);

            if (_snapshots.TryGetValue(go, out var snapshot)) {
                snapshot.Restore();
            }

            var identity = go.GetComponent<PoolObjectIdentity>();
            if (null == identity) {
                identity = go.AddComponent<PoolObjectIdentity>();
                identity.AssetPath = _poolName;
                identity.UniqueId = NewUniqueId;
#if DEBUG_SPAWNPOOLS
                Logger.Info("Pool object(id: {0}, path: {1}) created.",
                    identity.UniqueId, identity.AssetPath);
#endif
            }
            identity.IsPooling = false;
        }

        protected virtual bool ObjectPreprocessBeforeRecycle(GameObject go) {
            var identity = go.GetComponent<PoolObjectIdentity>();
            if (null == identity) {
                Logger.Error("Not a valid pool object: " + go);
                return false;
            }

            if (identity.AssetPath != _poolName) {
                Logger.Error("Object to recycle does not match the pool name, require: {0}, get: {1}",
                    _poolName, identity.AssetPath);
                return false;
            }
            identity.IsPooling = true;

            if (!_snapshots.TryGetValue(go, out _)) {
                Logger.Warning("No target snapshot in the pool: " + identity.AssetPath);
                identity.Destroyed = true;
                return false;
            }

            return true;
        }

        protected virtual void ObjectPostprocessAfterRecycle(GameObject go) {
            go.transform.SetParent(_poolGo.transform, false);
            HideGameObject(go);
        }

        private void ShowGameObject(GameObject go) {
            switch (_spawnPools.PoolsSetting.HiddenType) {
                case SpawnPoolsSetting.PoolObjectHiddenType.Deactive:
                    go.SetActive(true);
                    break;
                case SpawnPoolsSetting.PoolObjectHiddenType.Position:
                    go.transform.ClearAllTrailRenderers();
                    go.transform.StartAllParticleSystems();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        "Unknown hidden type: " + _spawnPools.PoolsSetting.HiddenType);
            }
        }

        private void HideGameObject(GameObject go) {
            switch (_spawnPools.PoolsSetting.HiddenType) {
                case SpawnPoolsSetting.PoolObjectHiddenType.Deactive:
                    go.transform.ClearAllTrailRenderers();
                    go.SetActive(false);
                    break;
                case SpawnPoolsSetting.PoolObjectHiddenType.Position:
                    go.transform.StopAllParticleSystems();
                    go.transform.ClearAllTrailRenderers();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        "Unknown hidden type: " + _spawnPools.PoolsSetting.HiddenType);
            }
        }

        protected virtual void ObjectPreprocessBeforeDestroy(GameObject go) {
            if (!go) {
                return;
            }

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