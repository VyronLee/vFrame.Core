//------------------------------------------------------------
//        File:  DefaultGameObjectBuilder.cs
//       Brief:  DefaultGameObjectBuilder
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-02-18 15:04
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System;
using System.Collections;
using UnityEngine;
using vFrame.Core.Base;
using vFrame.Core.ObjectPools;
using Object = UnityEngine.Object;

namespace vFrame.Core.Unity.SpawnPools
{
    internal class DefaultGameObjectLoaderFromPath : BaseObject<string>, IGameObjectLoaderFromPath
    {
        private string _path;

        public GameObject Load() {
            var prefab = Resources.Load<GameObject>(_path);
            if (!prefab) {
                throw new AssetLoadFailedException(_path);
            }
            var obj = Object.Instantiate(prefab);
            Object.DontDestroyOnLoad(obj);
            return obj;
        }

        public LoadAsyncRequest LoadAsync() {
            return DefaultLoadAsyncRequest.Create(_path);
        }

        public void Create() {
            throw new NotImplementedException();
        }

        protected override void OnCreate(string arg1) {
            _path = arg1;
        }

        protected override void OnDestroy() {
            _path = null;
        }

        private class DefaultLoadAsyncRequest : LoadAsyncRequest
        {
            private string _path;
            private ResourceRequest _request;

            public static DefaultLoadAsyncRequest Create(string path) {
                var request = ObjectPool<DefaultLoadAsyncRequest>.Shared.Get();
                request.Create();
                request._path = path;
                return request;
            }

            protected override void OnDestroy() {
                base.OnDestroy();

                _request = null;
                _path = null;
                ObjectPool<DefaultLoadAsyncRequest>.Shared.Return(this);
            }

            protected override IEnumerator OnProcessLoad() {
                _request = Resources.LoadAsync<GameObject>(_path);
                yield return _request;
                var prefab = _request.asset as GameObject;
                if (!prefab) {
                    throw new AssetLoadFailedException(_path);
                }
                GameObject = Object.Instantiate(prefab);
                Object.DontDestroyOnLoad(GameObject);
            }
        }
    }
}