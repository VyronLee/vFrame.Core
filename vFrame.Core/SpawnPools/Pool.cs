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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vFrame.Core.Base;
using vFrame.Core.Extensions.UnityEngine;
using vFrame.Core.SpawnPools.Behaviours;
using vFrame.Core.SpawnPools.Builders;
using vFrame.Core.SpawnPools.Snapshots;
using Logger = vFrame.Core.Loggers.Logger;
using Object = UnityEngine.Object;

namespace vFrame.Core.SpawnPools
{
    public class Pool : BaseObject<string, int, IGameObjectBuilder>, IPool
    {
        private readonly Queue<GameObject> _objects = new Queue<GameObject>();
        private int _lastTime;
        private GameObject _poolGo;

        private int _lifetime;
        private string _poolName;
        private IGameObjectBuilder _builder;

        private int _uniqueId;
        private int NewUniqueId => ++_uniqueId;
        private Dictionary<GameObject, PoolObjectSnapshot> _snapshots;

        public int SpawnedTimes { get; private set; }

        protected override void OnCreate(string poolName, int lifetime, IGameObjectBuilder builder) {
            _lifetime = lifetime;
            _lastTime = Time.frameCount;
            _poolName = poolName;
            _builder = builder;
            _snapshots = new Dictionary<GameObject, PoolObjectSnapshot>(32);

            _poolGo = new GameObject(string.Format("Pool({0})", poolName));
            _poolGo.transform.SetParent(SpawnPools.PoolsParent.transform, false);
        }

        protected override void OnDestroy() {
            _snapshots?.Clear();
            _snapshots = null;
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

            if (_poolGo) {
                Object.Destroy(_poolGo);
            }
            _poolGo = null;

#if DEBUG_SPAWNPOOLS
            Logger.Info("Spawn pool cleared: {0}", _poolName);
#endif
        }

        public GameObject Spawn() {
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

            if (null == obj) {
#if DEBUG_SPAWNPOOLS
                Debug.LogFormat("No objects in pool({0}), spawning new one..", _poolName);
#endif
                obj = _builder.Spawn();

                var snapshot = new PoolObjectSnapshot();
                snapshot.Create(obj);
                snapshot.Take();
                _snapshots.Add(obj, snapshot);
            }

            return OnSpawn(obj);
        }

        public IEnumerator SpawnAsync(Action<Object> callback) {
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

            if (null == obj) {
#if DEBUG_SPAWNPOOLS
                Debug.LogFormat("No objects in pool({0}), spawning new one..", _poolName);
#endif
                yield return _builder.SpawnAsync(v => obj = v);

                var snapshot = new PoolObjectSnapshot();
                snapshot.Create(obj);
                snapshot.Take();
                _snapshots.Add(obj, snapshot);
            }

            OnSpawn(obj);
            callback(obj);
        }

        private GameObject OnSpawn(GameObject obj) {
            ++SpawnedTimes;
            _lastTime = Time.frameCount;

            ObjectPreprocessBeforeReturn(obj);

            return obj;
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
            return Time.frameCount - _lastTime > _lifetime;
        }

        protected virtual void ObjectPreprocessBeforeReturn(GameObject go) {
            go.transform.SetParent(null, false);

            switch (SpawnPoolsSetting.HiddenType) {
                case SpawnPoolsSetting.PoolObjectHiddenType.Deactive:
                    go.SetActive(true);
                    break;
                case SpawnPoolsSetting.PoolObjectHiddenType.Position:
                    go.transform.EnableAllAnimations();
                    go.transform.EnableAllAnimators();
                    go.transform.EnableAllParticleSystems();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        "Unknown hidden type: " + SpawnPoolsSetting.HiddenType);
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
                //Logger.Warning("No target snapshot in the pool: " + go);
                identity.Destroyed = true;
                return false;
            }

            return true;
        }

        protected virtual void ObjectPostprocessAfterRecycle(GameObject go) {
            go.transform.SetParent(_poolGo.transform, false);

            if (_snapshots.TryGetValue(go, out var snapshot)) {
                snapshot.Restore();
            }

            switch (SpawnPoolsSetting.HiddenType) {
                case SpawnPoolsSetting.PoolObjectHiddenType.Deactive:
                    go.SetActive(false);
                    break;
                case SpawnPoolsSetting.PoolObjectHiddenType.Position:
                    go.transform.DisableAllAnimators();
                    go.transform.DisableAllAnimations();
                    go.transform.DisableAllParticleSystems();
                    go.transform.ClearAllTrailRenderers();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        "Unknown hidden type: " + SpawnPoolsSetting.HiddenType);
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