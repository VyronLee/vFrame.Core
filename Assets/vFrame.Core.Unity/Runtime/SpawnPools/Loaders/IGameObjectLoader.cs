//------------------------------------------------------------
//        File:  IGameObjectBuilder.cs
//       Brief:  IGameObjectBuilder
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-02-18 14:49
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using UnityEngine;

namespace vFrame.Core.Unity.SpawnPools
{
    public interface IGameObjectLoader
    {
        GameObject Load();
        LoadAsyncRequest LoadAsync();
    }
}