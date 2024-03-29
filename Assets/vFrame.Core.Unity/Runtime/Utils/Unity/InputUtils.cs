//------------------------------------------------------------
//        File:  InputUtility.cs
//       Brief:  Input工具类
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2018-12-24 11:52
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace vFrame.Core.Unity.Utils
{
    public static class InputUtils
    {
        /// <summary>
        ///     是否点击在UI物件之上
        /// </summary>
        /// <param name="screenPosition">屏幕坐标</param>
        /// <returns></returns>
        public static bool IsPointOverUIObject(Vector2 screenPosition) {
            var eventData = new PointerEventData(EventSystem.current) {
                position = new Vector2(screenPosition.x, screenPosition.y)
            };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
        }
    }
}