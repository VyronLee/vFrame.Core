//------------------------------------------------------------
//        File:  LoadAsyncRequestOnLoaded.cs
//       Brief:  LoadAsyncRequestOnLoaded
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2021-03-22 15:51
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using UnityEngine;

namespace vFrame.Core.Unity.SpawnPools
{
    public class LoadAsyncRequestOnLoaded : LoadAsyncRequest
    {
        public override float Progress => IsDone ? 1f : 0f;

        protected override bool Validate(out GameObject obj) {
            return obj = GameObject;
        }
    }
}