//------------------------------------------------------------
//        File:  DefaultAssetsProvider.cs
//       Brief:  DefaultAssetsProvider
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-02-18 15:04
//   Copyright:  Copyright (c) 2018, VyronLee
//============================================================
using System;
using UnityEngine;
using vFrame.Core.Interface.SpawnPools;
using Object = UnityEngine.Object;

namespace vFrame.Core.SpawnPools.Provider
{
    public class DefaultAssetsProvider : IAssetsProvider
    {
        public Object Load(string assetPath, Type type)
        {
            var prefab = Resources.Load(assetPath, type);
            if (!prefab)
                throw new Exception("Load asset failed: " + assetPath);
            return Object.Instantiate(prefab);
        }
    }
}