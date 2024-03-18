//------------------------------------------------------------
//        File:  GameObjectUtility.cs
//       Brief:  GameObject工具类
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-01-04 17:20
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using UnityEngine;

namespace vFrame.Core.Unity.Extensions
{
    public static class GameObjectExtension
    {
        /// <summary>
        /// 设置物件以及子物件的Layer
        /// </summary>
        /// <param name="go"></param>
        /// <param name="layer"></param>
        public static void SetLayerRecursive(this GameObject go, int layer) {
            for (var i = 0; i < go.transform.childCount; ++i) {
                SetLayerRecursive(go.transform.GetChild(i).gameObject, layer);
            }
            go.layer = layer;
        }

        /// <summary>
        /// 设置物件以及子物件的Tag
        /// </summary>
        /// <param name="go"></param>
        /// <param name="tag"></param>
        public static void SetTagRecursive(this GameObject go, string tag) {
            for (var i = 0; i < go.transform.childCount; ++i) {
                SetTagRecursive(go.transform.GetChild(i).gameObject, tag);
            }
            go.tag = tag;
        }

        /// <summary>
        /// 获取或者添加组件
        /// </summary>
        /// <param name="go"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(this GameObject go) where T: MonoBehaviour {
            var comp = go.GetComponent<T>();
            if (null == comp) {
                comp = go.AddComponent<T>();
            }
            return comp;
        }

        /// <summary>
        /// 扩展销毁方法
        /// </summary>
        /// <param name="go"></param>
        public static void DestroyEx(this Object go) {
            if (Application.isPlaying) {
                Object.Destroy(go);
                return;
            }
            Object.DestroyImmediate(go);
        }
    }
}