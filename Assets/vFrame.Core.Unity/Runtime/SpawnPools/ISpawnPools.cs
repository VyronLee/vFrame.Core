//------------------------------------------------------------
//        File:  ISpawnPools.cs
//       Brief:  Spawn pools interface.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-08 23:47
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using UnityEngine;

namespace vFrame.Core.Unity.SpawnPools
{
    public interface ISpawnPools
    {
        void Update();
        GameObject Spawn(string assetPath, Transform parent = null);
        ILoadAsyncRequest SpawnAsync(string assetPath, Transform parent = null);
        void Recycle(GameObject obj);
        IPreloadAsyncRequest PreloadAsync(string[] assetPaths);
    }
}