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
using Object = UnityEngine.Object;

namespace vFrame.Core.SpawnPools.Builders
{
    internal class DefaultGameObjectBuilderFromPath : BaseObject<string>, IGameObjectBuilderFromPath
    {
        private string _path;

        protected override void OnCreate(string arg1) {
            _path = arg1;
        }

        protected override void OnDestroy() {
            _path = null;
        }

        public GameObject Spawn() {
            var prefab = Resources.Load<GameObject>(_path);
            if (!prefab)
                throw new Exception("Load asset failed: " + _path);
            return Object.Instantiate(prefab);
        }

        public IEnumerator SpawnAsync(Action<GameObject> callback) {
            var request = Resources.LoadAsync<GameObject>(_path);
            yield return request;
            callback(request.asset as GameObject);
        }
    }
}