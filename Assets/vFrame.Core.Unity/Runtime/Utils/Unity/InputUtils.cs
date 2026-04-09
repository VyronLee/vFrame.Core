// ------------------------------------------------------------
//         File: InputUtils.cs
//        Brief: Utility class for input-related queries.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2018-12-24 11:52:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace vFrame.Core.Unity
{
    public static class InputUtils
    {
        /// <summary>
        ///     Determines whether the given screen position is over a UI object.
        /// </summary>
        /// <param name="screenPosition">Screen-space position to test.</param>
        /// <returns><c>true</c> if a UI object is hit at the specified position; otherwise, <c>false</c>.</returns>
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
