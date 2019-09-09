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
using vFrame.Core.Extensions;
using UnityEngine;
using vFrame.Core.Extensions.UnityEngine;
using Object = UnityEngine.Object;

namespace vFrame.Core.SpawnPools.Pools
{
    public abstract class PoolBase<T> where T : Object
    {
        protected readonly Stack<T> _objects = new Stack<T>();
        protected int _lasttime;
        protected GameObject _poolGo;

        private readonly int _lifetime;
        private readonly string _poolName;

        private Vector3 _originLocalPosition;
        private Vector3 _originLocalScale;
        private Quaternion _originLocalRotation;

        public int SpawnedTimes { get; private set; }

        protected PoolBase(string poolName, int lifetime)
        {
            _lifetime = lifetime;
            _lasttime = Time.frameCount;
            _poolName = poolName;

            _poolGo = new GameObject(string.Format("Pool({0})", poolName));
            _poolGo.transform.SetParent(SpawnPools<T>.PoolsParent.transform, false);
        }

        public void Clear()
        {
            foreach (var obj in _objects)
                Object.Destroy(obj);
            _objects.Clear();
        }

        public T Spawn()
        {
            T obj;
            if (_objects.Count > 0)
            {
#if DEBUG_SPAWNPOOLS
                Debug.LogFormat("Spawning object from pool({0}) ", _poolName);
#endif
                obj = _objects.Pop();
            }
            else
            {
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

        public IEnumerator SpawnAsync(Action<Object> callback)
        {
            T obj = null;
            if (_objects.Count > 0)
            {
#if DEBUG_SPAWNPOOLS
                Debug.LogFormat("Spawning object from pool({0}) ", _poolName);
#endif
                obj = _objects.Pop();
            }
            else
            {
#if DEBUG_SPAWNPOOLS
                Debug.LogFormat("No objects in pool({0}), spawning new one..", _poolName);
#endif
                yield return DoSpawnAsync(v => obj = v);

                var go = obj as GameObject;
                if (go)
                {
                    _originLocalPosition = go.transform.localPosition;
                    _originLocalScale = go.transform.localScale;
                    _originLocalRotation = go.transform.localRotation;
                }
            }
            OnSpawn(obj);
            callback(obj);
        }

        private T OnSpawn(T obj)
        {
            ++SpawnedTimes;
            _lasttime = Time.frameCount;

            ObjectPreprocessBeforeReturn(obj);

            return obj;
        }

        public void Recycle(T obj)
        {
#if DEBUG_SPAWNPOOLS
            Debug.LogFormat("Recycling object into pool({0})", _assetName);
#endif
            _objects.Push(obj);

            ObjectPostprocessAfterRecycle(obj);
        }

        public int Count
        {
            get { return _objects.Count; }
        }

        public bool IsTimeout()
        {
            return Time.frameCount - _lasttime > _lifetime;
        }

        private void ObjectPreprocessBeforeReturn(Object obj)
        {
            var go = obj as GameObject;
            if (!go)
                return;

            switch (SpawnPoolsSetting.HiddenType)
            {
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
                    go.transform.SetParent(_poolGo.transform, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        "Unknown hidden type: " + SpawnPoolsSetting.HiddenType);
            }
        }

        private void ObjectPostprocessAfterRecycle(Object obj)
        {
            var go = obj as GameObject;
            if (!go)
                return;

            go.transform.SetParent(_poolGo.transform, false);

            switch (SpawnPoolsSetting.HiddenType)
            {
                case SpawnPoolsSetting.PoolObjectHiddenType.Deactive:
                    go.SetActive(false);
                    break;
                case SpawnPoolsSetting.PoolObjectHiddenType.Position:
                    go.transform.localPosition = Vector3.zero;
                    go.transform.DisableAllAnimators();
                    go.transform.DisableAllAnimations();
                    go.transform.DisableAllParticleSystems();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        "Unknown hidden type: " + SpawnPoolsSetting.HiddenType);
            }
        }

        protected abstract T DoSpawn();
        protected abstract IEnumerator DoSpawnAsync(Action<T> callback);
    }
}