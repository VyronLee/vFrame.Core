//------------------------------------------------------------
//        File:  DefaultAssetsProviderAsync.cs
//       Brief:  DefaultAssetsProviderAsync
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-02-18 15:04
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace vFrame.Core.SpawnPools.Provider
{
    public class DefaultAssetsProviderAsync : IAssetsProviderAsync
    {
        public IEnumerator LoadAsync(string assetPath, Type type, Action<Object> callback) {
            var request = Resources.LoadAsync(assetPath, type);
            yield return request;
            callback(request.asset);
        }
    }
}