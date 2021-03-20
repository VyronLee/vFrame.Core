//------------------------------------------------------------
//        File:  IGameObjectBuilder.cs
//       Brief:  IGameObjectBuilder
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-02-18 14:49
//   Copyright:  Copyright (c) 2018, VyronLee
//============================================================

using UnityEngine;
using vFrame.Core.Base;

namespace vFrame.Core.SpawnPools.Loaders
{
    public interface IGameObjectLoader : IBaseObject
    {
        GameObject Load();
        LoadAsyncRequest LoadAsync();
    }
}