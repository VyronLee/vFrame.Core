//------------------------------------------------------------
//        File:  RectTransformExtension.cs
//       Brief:  RectTransform扩展
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-02-28 14:34
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using UnityEngine;

namespace vFrame.Core.Extensions.UnityEngine.UI
{
    public static class RectTransformExtension
    {
        public static void ScreenAdaptation(this RectTransform rect) {
#if UNITY_IPHONE || UNITY_IOS
            var screenPoint = (float)Screen.width / Screen.height;
#else
            var screenPoint = 1;
#endif
            //适配屏幕
            if (screenPoint < 1.78)
                return;

            //长屏幕，需要左右空出像素
            if (Mathf.Approximately(rect.anchorMin.x, 0f) && Mathf.Approximately(rect.anchorMax.x, 0f)) {
                //靠左的面板
                rect.anchoredPosition = rect.anchoredPosition + new Vector2(100f, 0f);
            }
            else if (Mathf.Approximately(rect.anchorMin.x, 1f) && Mathf.Approximately(rect.anchorMax.x, 1f)) {
                //靠右的面板
                rect.anchoredPosition = rect.anchoredPosition - new Vector2(100f, 0f);
            }
        }

        /// <summary>
        /// 判断两个RectTransform在世界座标下是否重叠
        /// </summary>
        /// <param name="rc1"></param>
        /// <param name="rc2"></param>
        /// <returns></returns>
        private static readonly Vector3[] _corners_1 = new Vector3[4];

        private static readonly Vector3[] _corners_2 = new Vector3[4];

        public static bool OverlapsInWorldSpace(this RectTransform rc1, RectTransform rc2) {
            rc1.GetWorldCorners(_corners_1);
            rc2.GetWorldCorners(_corners_2);

            var r1 = new Rect(_corners_1[0].x, _corners_1[0].y, _corners_1[2].x - _corners_1[0].x,
                _corners_1[2].y - _corners_1[0].y);
            var r2 = new Rect(_corners_2[0].x, _corners_2[0].y, _corners_2[2].x - _corners_2[0].x,
                _corners_2[2].y - _corners_2[0].y);

            return r1.Overlaps(r2);
        }
    }
}