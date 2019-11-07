//------------------------------------------------------------
//        File:  IAssetsProvider.cs
//       Brief:  IAssetsProvider
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-02-18 14:49
//   Copyright:  Copyright (c) 2018, VyronLee
//============================================================

using System;
using System.Collections;
using Object = UnityEngine.Object;

namespace vFrame.Core.SpawnPools
{
    public interface IAssetsProvider
    {
        Object Load(string assetPath, Type type);
    }

    public interface IAssetsProviderAsync
    {
        IEnumerator LoadAsync(string assetPath, Type type, Action<Object> callback);
    }
}