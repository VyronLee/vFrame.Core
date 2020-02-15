//------------------------------------------------------------
//        File:  ISpawnPools.cs
//       Brief:  Spawn pools interface.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-09-08 23:47
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using UnityEngine;

namespace vFrame.Core.SpawnPools
{
    public interface ISpawnPools
    {
        IPool this[string assetName] { get; }
        IPool this[GameObject prefab] { get; }
        void Update();
    }
}