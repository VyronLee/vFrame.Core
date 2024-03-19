//------------------------------------------------------------
//        File:  DefaultGameObjectBuilder.cs
//       Brief:  DefaultGameObjectBuilder
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-02-18 15:04
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using UnityEngine;
using vFrame.Core.Base;
using Object = UnityEngine.Object;

namespace vFrame.Core.Unity.SpawnPools
{
    internal class DefaultGameObjectLoader : CreateAbility<DefaultGameObjectLoader, string>, IGameObjectLoader
    {
        private string _path;

        public GameObject Load() {
            var prefab = Resources.Load<GameObject>(_path);
            if (!prefab) {
                throw new AssetLoadFailedException(_path);
            }
            return Object.Instantiate(prefab);
        }

        public LoadAsyncRequest LoadAsync() {
            var request = new DefaultLoadAsyncRequest();
            request.Path = _path;
            return request;
        }

        protected override void OnCreate(string arg1) {
            _path = arg1;
        }

        protected override void OnDestroy() {
            _path = null;
        }

        private class DefaultLoadAsyncRequest : LoadAsyncRequest
        {
            private ResourceRequest _request;
            internal string Path { get; set; }
            public override float Progress => _request?.progress ?? 0;

            protected override void OnDestroy() {
                _request = null;
                Path = null;
                base.OnDestroy();
            }

            protected override bool Validate(out GameObject obj) {
                if (null == _request) {
                    _request = Resources.LoadAsync<GameObject>(Path);
                }
                if (!_request.isDone) {
                    obj = null;
                    return false;
                }

                var prefab = _request.asset as GameObject;
                if (!prefab) {
                    throw new AssetLoadFailedException(Path);
                }
                obj = Object.Instantiate(prefab);
                return true;
            }
        }
    }
}