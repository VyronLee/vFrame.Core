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
using vFrame.Core.SpawnPools.Behaviours;
using vFrame.Core.SpawnPools.Loaders;
using Object = UnityEngine.Object;
using Debug = vFrame.Core.SpawnPools.SpawnPoolDebug;

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

        private Action<GameObject> _onGetGameObject;
        private Action<GameObject> _onLoadCallbackImmediately;

        private CoroutinePool _coroutinePool;

        private int _spawnedTimes;
        internal int SpawnedTimes => _spawnedTimes;

        protected override void OnCreate(string poolName, SpawnPools spawnPools, IGameObjectLoader builder) {
            _spawnPools = spawnPools;
            _lastTime = Time.frameCount;
            _poolName = poolName;
            _builder = builder;
            _onGetGameObject = OnGetGameObject;
            _onLoadCallbackImmediately = OnLoadCallback;

            _poolGo = new GameObject($"Pool({poolName})");
            _poolGo.transform.SetParent(_spawnPools.PoolsParent.transform, false);

            _coroutinePool = _spawnPools.CoroutinePool;
        }

        protected override void OnDestroy() {
            if (_poolGo) {
                Object.Destroy(_poolGo);
            }
            _poolGo = null;
        }

        public void Clear() {
            foreach (var obj in _objects) {
                if (!obj) {
                    continue;
                }
                ObjectPreprocessBeforeDestroy(obj);
                Object.Destroy(obj);
            }
            _objects.Clear();

            Debug.Log("Spawn pool cleared: {0}", _poolName);
        }

        private GameObject TryGetFromPool() {
            GameObject obj = null;
            while (_objects.Count > 0) {
                Debug.Log("Spawning object from pool({0}) ", _poolName);
                obj = _objects.Dequeue();
                if (null == obj) {
                    Debug.Warning(
                        "Spawn object from pool, but obj == null, DON'T destroy managed object outside the pool!");
                }
                else {
                    break;
                }
            }
            return obj;
        }

        public GameObject Spawn() {
            var obj = TryGetFromPool();
            if (!obj) {
                Debug.Log("No objects in pool({0}), spawning new one..", _poolName);
                obj = _builder.Load();
                OnLoadCallback(obj);
            }

            OnGetGameObject(obj);
            return obj;
        }

        public ILoaderAsyncRequest SpawnAsync() {
            var obj = TryGetFromPool();
            if (!obj) {
                Debug.Log("No objects in pool({0}), spawning new one..", _poolName);
            }

            var request = null != obj ? LoadAsyncRequestOnLoaded.Create(obj) : _builder.LoadAsync();
            request.OnLoadCallback = _onLoadCallbackImmediately;
            request.OnGetGameObject = _onGetGameObject;
            request.Setup(_coroutinePool);
            return request;
        }

        private void OnLoadCallback(GameObject obj) {
            if (!obj) {
                Debug.Warning("Load gameObject callback, but target == null, pool name: " + _poolName);
                return;
            }
            HideGameObject(obj);
        }

        private void OnGetGameObject(GameObject obj) {
            _spawnedTimes += 1;
            _lastTime = Time.frameCount;

            if (!obj) {
                Debug.Warning("Get gameObject callback, but target == null, pool name: " + _poolName);
                return;
            }
            ObjectPreprocessBeforeReturn(obj);
        }

        public void Recycle(GameObject obj) {
            if (null == obj) {
                Debug.Error("Object to recycle cannot be null!");
                return;
            }
            
            Debug.Log("Recycling object into pool({0})", obj.name);
            
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

            var identity = go.GetComponent<PoolObjectIdentity>();
            if (null == identity) {
                identity = go.AddComponent<PoolObjectIdentity>();
                identity.AssetPath = _poolName;
                identity.UniqueId = NewUniqueId;
                Debug.Log("Pool object(id: {0}, path: {1}) created.", identity.UniqueId, identity.AssetPath);
            }
            identity.IsPooling = false;
        }

        protected virtual bool ObjectPreprocessBeforeRecycle(GameObject go) {
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
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        "Unknown hidden type: " + _spawnPools.PoolsSetting.HiddenType);
            }
        }

        private void HideGameObject(GameObject go) {
            if (!go) {
                return;
            }
            switch (_spawnPools.PoolsSetting.HiddenType) {
                case SpawnPoolsSetting.PoolObjectHiddenType.Deactive:
                    go.SetActive(false);
                    break;
                case SpawnPoolsSetting.PoolObjectHiddenType.Position:
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

            Debug.Log("Pool object(id: {0}, path: {1}) destroyed.", identity.UniqueId, identity.AssetPath);

            identity.Destroyed = true;
            Object.Destroy(identity);
        }
    }
}