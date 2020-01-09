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
using vFrame.Core.Extensions.UnityEngine;
using Logger = vFrame.Core.Loggers.Logger;
using Object = UnityEngine.Object;

namespace vFrame.Core.SpawnPools.Pools
{
    public abstract class PoolBase<T> where T : Object
    {
        protected readonly Queue<T> _objects = new Queue<T>();
        protected int _lasttime;
        protected GameObject _poolGo;

        private readonly int _lifetime;
        private readonly string _poolName;

        private Vector3 _originLocalPosition;
        private Vector3 _originLocalScale;
        private Quaternion _originLocalRotation;

        private int _uniqueId;
        protected int NewUniqueId => ++_uniqueId;

        public int SpawnedTimes { get; private set; }

        protected PoolBase(string poolName, int lifetime) {
            _lifetime = lifetime;
            _lasttime = Time.frameCount;
            _poolName = poolName;

            _poolGo = new GameObject(string.Format("Pool({0})", poolName));
            _poolGo.transform.SetParent(SpawnPools<T>.PoolsParent.transform, false);
        }

        public void Clear() {
            foreach (var obj in _objects) {
                ObjectPreprocessBeforeDestroy(obj);
                Object.Destroy(obj);
            }
            _objects.Clear();

#if DEBUG_SPAWNPOOLS
            Logger.Info("Spawn pool cleared: {0}", _poolName);
#endif
        }

        public T Spawn() {
            T obj = null;
            while (_objects.Count > 0) {
#if DEBUG_SPAWNPOOLS
                Debug.LogFormat("Spawning object from pool({0}) ", _poolName);
#endif
                obj = _objects.Dequeue();
                if (null == obj) {
                    Loggers.Logger.Warning(
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
                obj = DoSpawn();

                var go = obj as GameObject;
                if (!go)
                    return OnSpawn(obj);

                _originLocalPosition = go.transform.localPosition;
                _originLocalScale = go.transform.localScale;
                _originLocalRotation = go.transform.localRotation;
            }

            return OnSpawn(obj);
        }

        public IEnumerator SpawnAsync(Action<Object> callback) {
            T obj = null;
            while (_objects.Count > 0) {
#if DEBUG_SPAWNPOOLS
                Debug.LogFormat("Spawning object from pool({0}) ", _poolName);
#endif
                obj = _objects.Dequeue();
                if (null == obj) {
                    Loggers.Logger.Warning(
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
                yield return DoSpawnAsync(v => obj = v);

                var go = obj as GameObject;
                if (go) {
                    _originLocalPosition = go.transform.localPosition;
                    _originLocalScale = go.transform.localScale;
                    _originLocalRotation = go.transform.localRotation;
                }
            }

            OnSpawn(obj);
            callback(obj);
        }

        private T OnSpawn(T obj) {
            ++SpawnedTimes;
            _lasttime = Time.frameCount;

            ObjectPreprocessBeforeReturn(obj);

            return obj;
        }

        public void Recycle(T obj) {
            if (null == obj) {
                Logger.Error("Item to recycle cannot be null!");
                return;
            }
#if DEBUG_SPAWNPOOLS
            Debug.LogFormat("Recycling object into pool({0})", _assetName);
#endif
            if (!ObjectPreprocessBeforeRecycle(obj)) {
                return;
            }

            _objects.Enqueue(obj);

            ObjectPostprocessAfterRecycle(obj);
        }

        public int Count {
            get { return _objects.Count; }
        }

        public bool IsTimeout() {
            return Time.frameCount - _lasttime > _lifetime;
        }

        protected virtual void ObjectPreprocessBeforeReturn(Object obj) {
            var go = obj as GameObject;
            if (null == go)
                return;

            go.transform.SetParent(null, false);

            switch (SpawnPoolsSetting.HiddenType) {
                case SpawnPoolsSetting.PoolObjectHiddenType.Deactive:
                    go.SetActive(true);
                    break;
                case SpawnPoolsSetting.PoolObjectHiddenType.Position:
                    go.transform.localPosition = _originLocalPosition;
                    go.transform.localScale = _originLocalScale;
                    go.transform.localRotation = _originLocalRotation;
                    go.transform.EnableAllAnimations();
                    go.transform.EnableAllAnimators();
                    go.transform.EnableAllParticleSystems();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        "Unknown hidden type: " + SpawnPoolsSetting.HiddenType);
            }
        }

        protected virtual bool ObjectPreprocessBeforeRecycle(Object obj) {
            return true;
        }

        protected virtual void ObjectPostprocessAfterRecycle(Object obj) {
            var go = obj as GameObject;
            if (null == go)
                return;

            go.transform.SetParent(_poolGo.transform, false);

            switch (SpawnPoolsSetting.HiddenType) {
                case SpawnPoolsSetting.PoolObjectHiddenType.Deactive:
                    go.SetActive(false);
                    break;
                case SpawnPoolsSetting.PoolObjectHiddenType.Position:
                    go.transform.localPosition = _originLocalPosition;
                    go.transform.localScale = _originLocalScale;
                    go.transform.localRotation = _originLocalRotation;
                    go.transform.DisableAllAnimators();
                    go.transform.DisableAllAnimations();
                    go.transform.DisableAllParticleSystems();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        "Unknown hidden type: " + SpawnPoolsSetting.HiddenType);
            }
        }

        protected virtual void ObjectPreprocessBeforeDestroy(Object obj) {

        }

        protected abstract T DoSpawn();
        protected abstract IEnumerator DoSpawnAsync(Action<T> callback);
    }
}