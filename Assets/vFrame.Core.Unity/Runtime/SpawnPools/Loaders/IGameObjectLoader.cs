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
using vFrame.Core.Base;

namespace vFrame.Core.Unity.SpawnPools
{
    public interface IGameObjectLoader : IBaseObject
    {
        GameObject Load();
        LoadAsyncRequest LoadAsync();
    }
}