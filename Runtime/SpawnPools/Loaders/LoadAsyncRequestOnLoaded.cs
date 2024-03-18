//------------------------------------------------------------
//        File:  LoadAsyncRequestOnLoaded.cs
//       Brief:  LoadAsyncRequestOnLoaded
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2021-03-22 15:51
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Collections;
using UnityEngine;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.Unity.SpawnPools
{
    public class LoadAsyncRequestOnLoaded : LoadAsyncRequest
    {
        public static LoadAsyncRequestOnLoaded Create(GameObject obj) {
            var request = ObjectPool<LoadAsyncRequestOnLoaded>.Shared.Get();
            request.Create();
            request.GameObject = obj;
            return request;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            ObjectPool<LoadAsyncRequestOnLoaded>.Shared.Return(this);
        }

        protected override IEnumerator OnProcessLoad() {
            IsFinished = true;

#pragma warning disable 162
            if (false) { // break is not required, otherwise it will wait for 1 frame
                yield break;
            }
#pragma warning restore 162
        }
    }
}