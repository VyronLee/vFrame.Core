//------------------------------------------------------------
//        File:  GameObjectUtility.cs
//       Brief:  GameObject工具类
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-01-04 17:20
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using UnityEngine;

namespace vFrame.Core.Extensions.UnityEngine
{
    public static class GameObjectExtension
    {
        /// <summary>
        /// 设置物件以及子物件的Layer
        /// </summary>
        /// <param name="go"></param>
        /// <param name="layer"></param>
        public static void SetLayerRecursive(this GameObject go, int layer)
        {
            for (var i = 0; i < go.transform.childCount; ++i)
                SetLayerRecursive(go.transform.GetChild(i).gameObject, layer);

            go.layer = layer;
        }
        
        /// <summary>
        /// 设置物件以及子物件的Tag
        /// </summary>
        /// <param name="go"></param>
        /// <param name="tag"></param>
        public static void SetTagRecursive(this GameObject go, string tag)
        {
            for (var i = 0; i < go.transform.childCount; ++i)
                SetTagRecursive(go.transform.GetChild(i).gameObject, tag);

            go.tag = tag;
        }
    }
}