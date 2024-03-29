//------------------------------------------------------------
//        File:  RectTransformExtension.cs
//       Brief:  RectTransform扩展
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-02-28 14:34
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using UnityEngine;

namespace vFrame.Core.Unity.Extensions
{
    public static class RectTransformExtension
    {
        /// <summary>
        ///     判断两个RectTransform在世界座标下是否重叠
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

        #region Left, Right, Top, Bottom

        public static void SetLeft(this RectTransform transform, float left) {
            transform.offsetMin = new Vector2(left, transform.offsetMin.y);
        }

        public static float GetLeft(this RectTransform transform) {
            return transform.offsetMin.x;
        }

        public static void SetRight(this RectTransform transform, float right) {
            transform.offsetMax = new Vector2(-right, transform.offsetMax.y);
        }

        public static float GetRight(this RectTransform transform) {
            return -transform.offsetMax.x;
        }

        public static void SetTop(this RectTransform transform, float top) {
            transform.offsetMax = new Vector2(transform.offsetMax.x, -top);
        }

        public static float GetTop(this RectTransform transform) {
            return -transform.offsetMax.y;
        }

        public static void SetBottom(this RectTransform transform, float bottom) {
            transform.offsetMin = new Vector2(transform.offsetMin.x, bottom);
        }

        public static float GetBottom(this RectTransform transform) {
            return transform.offsetMin.y;
        }

        public static void SetLeftTopRightBottom(this RectTransform transform, float left, float top, float right,
            float bottom) {
            transform.offsetMin = new Vector2(left, bottom);
            transform.offsetMax = new Vector2(-right, -top);
        }

        #endregion

        #region PosX, PosY, Width, Height

        public static void SetPosX(this RectTransform transform, float posX) {
            transform.anchoredPosition = new Vector2(posX, transform.anchoredPosition.y);
        }

        public static void SetPosY(this RectTransform transform, float posY) {
            transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, posY);
        }

        public static void SetPosXY(this RectTransform transform, float posX, float posY) {
            transform.anchoredPosition = new Vector2(posX, posY);
        }

        public static void SetWidth(this RectTransform transform, float width) {
            transform.sizeDelta = new Vector2(width, transform.sizeDelta.y);
        }

        public static float GetWidth(this RectTransform transform) {
            return transform.rect.width;
        }

        public static void SetHeight(this RectTransform transform, float height) {
            transform.sizeDelta = new Vector2(transform.sizeDelta.x, height);
        }

        public static float GetHeight(this RectTransform transform) {
            return transform.rect.height;
        }

        public static void SetWidthHeight(this RectTransform transform, float width, float height) {
            transform.sizeDelta = new Vector2(width, height);
        }

        public static void SetPosAndSize(this RectTransform transform, float posX, float posY, float width,
            float height) {
            transform.anchoredPosition = new Vector2(posX, posY);
            transform.sizeDelta = new Vector2(width, height);
        }

        #endregion

        #region Anchor Offset

        public static void SetLeftAnchorOffset(this RectTransform transform, float leftPercent) {
            transform.anchorMin = new Vector2(leftPercent, transform.anchorMin.y);
        }

        public static void SetRightAnchorOffset(this RectTransform transform, float rightPercent) {
            transform.anchorMax = new Vector2(1f - rightPercent, transform.anchorMax.y);
        }

        public static void SetTopAnchorOffset(this RectTransform transform, float topPercent) {
            transform.anchorMax = new Vector2(transform.anchorMax.x, 1f - topPercent);
        }

        public static void SetBottomAnchorOffset(this RectTransform transform, float bottomPercent) {
            transform.anchorMin = new Vector2(transform.anchorMin.x, bottomPercent);
        }

        public static void SetAnchorOffset(this RectTransform transform, float left, float top, float right,
            float bottom) {
            transform.anchorMin = new Vector2(left, bottom);
            transform.anchorMax = new Vector2(1f - right, 1f - top);
        }

        #endregion
    }
}