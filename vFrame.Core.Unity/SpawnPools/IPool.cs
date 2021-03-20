//------------------------------------------------------------
//        File:  IPool.cs
//       Brief:  IPool
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-09-08 23:48
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vFrame.Core.SpawnPools
{
    public interface IPool
    {
        GameObject Spawn();
        GameObject Spawn(IEnumerable<Type> additional);
        ILoaderAsyncRequest SpawnAsync();
        ILoaderAsyncRequest SpawnAsync(IEnumerable<Type> additional);
        void Recycle(GameObject obj);
        int Count { get; }
    }

    public interface ILoaderAsyncRequest : IDisposable
    {
        GameObject GetGameObject();
        IEnumerator Await();
        bool IsFinished { get; }
    }
}