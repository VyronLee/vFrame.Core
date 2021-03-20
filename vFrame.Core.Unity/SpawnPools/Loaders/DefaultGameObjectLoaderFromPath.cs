//------------------------------------------------------------
//        File:  DefaultGameObjectBuilder.cs
//       Brief:  DefaultGameObjectBuilder
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-02-18 15:04
//   Copyright:  Copyright (c) 2018, VyronLee
//============================================================

using System;
using System.Collections;
using UnityEngine;
using vFrame.Core.Base;
using vFrame.Core.ObjectPools;
using Object = UnityEngine.Object;

namespace vFrame.Core.SpawnPools.Loaders
{
    internal class DefaultGameObjectLoaderFromPath : BaseObject<string>, IGameObjectLoaderFromPath
    {
        private string _path;

        protected override void OnCreate(string arg1) {
            _path = arg1;
        }

        protected override void OnDestroy() {
            _path = null;
        }

        public GameObject Load() {
            var prefab = Resources.Load<GameObject>(_path);
            if (!prefab)
                throw new Exception("Load asset failed: " + _path);
            return Object.Instantiate(prefab);
        }

        public LoadAsyncRequest LoadAsync() {
            return DefaultLoadAsyncRequest.Create(_path);
        }

        private class DefaultLoadAsyncRequest : LoadAsyncRequest
        {
            private string _path;
            private ResourceRequest _request;

            public static DefaultLoadAsyncRequest Create(string path) {
                var request = ObjectPool<DefaultLoadAsyncRequest>.Shared.Get();
                request._path = path;
                request.IsFinished = false;
                return request;
            }

            public override void Dispose() {
                _request = null;
                _path = null;
                IsFinished = false;
                ObjectPool<DefaultLoadAsyncRequest>.Shared.Return(this);
            }

            public override IEnumerator Await() {
                IsFinished = false;
                _request = Resources.LoadAsync<GameObject>(_path);
                yield return _request;
                var prefab = _request.asset as GameObject;
                if (!prefab)
                    throw new Exception("Load asset failed: " + _path);
                GameObject = Object.Instantiate(prefab);
                InvokeLoadCallback();
                IsFinished = true;
            }
        }
    }
}