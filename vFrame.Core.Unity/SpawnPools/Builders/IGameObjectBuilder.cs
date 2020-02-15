//------------------------------------------------------------
//        File:  IGameObjectBuilder.cs
//       Brief:  IGameObjectBuilder
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-02-18 14:49
//   Copyright:  Copyright (c) 2018, VyronLee
//============================================================

using System;
using System.Collections;
using UnityEngine;
using vFrame.Core.Base;

namespace vFrame.Core.SpawnPools.Builders
{
    public interface IGameObjectBuilder : IBaseObject
    {
        GameObject Spawn();
        IEnumerator SpawnAsync(Action<GameObject> callback);
    }
}