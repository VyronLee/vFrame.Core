//------------------------------------------------------------
//        File:  IPool.cs
//       Brief:  IPool
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-08 23:48
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using UnityEngine;

namespace vFrame.Core.Unity.SpawnPools
{
    internal interface IPool
    {
        int Count { get; }
        GameObject Spawn(Transform parent = null);
        ILoadAsyncRequest SpawnAsync(Transform parent = null);
        void Recycle(GameObject obj);
    }
}