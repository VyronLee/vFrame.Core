// ------------------------------------------------------------
//         File: RectTransformExtension.cs
//        Brief: Extension methods for RectTransform providing
//               convenient access to offsets, dimensions,
//               anchored positions, and anchor offsets.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-02-28 14:34:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using UnityEngine;

namespace vFrame.Core.Unity
{
    public static class RectTransformExtension
    {
        private static readonly Vector3[] _corners_1 = new Vector3[4];

        private static readonly Vector3[] _corners_2 = new Vector3[4];

        /// <summary>
        ///     Determines whether two RectTransforms overlap in world space.
        /// </summary>
        /// <param name="rc1">The first RectTransform to test.</param>
        /// <param name="rc2">The second RectTransform to test.</param>
        /// <returns><c>true</c> if the two rectangles overlap; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        ///     Sets the left offset of the RectTransform.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="left">The left offset value.</param>
        public static void SetLeft(this RectTransform transform, float left) {
            transform.offsetMin = new Vector2(left, transform.offsetMin.y);
        }

        /// <summary>
        ///     Gets the left offset of the RectTransform.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <returns>The left offset value.</returns>
        public static float GetLeft(this RectTransform transform) {
            return transform.offsetMin.x;
        }

        /// <summary>
        ///     Sets the right offset of the RectTransform.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="right">The right offset value.</param>
        public static void SetRight(this RectTransform transform, float right) {
            transform.offsetMax = new Vector2(-right, transform.offsetMax.y);
        }

        /// <summary>
        ///     Gets the right offset of the RectTransform.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <returns>The right offset value.</returns>
        public static float GetRight(this RectTransform transform) {
            return -transform.offsetMax.x;
        }

        /// <summary>
        ///     Sets the top offset of the RectTransform.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="top">The top offset value.</param>
        public static void SetTop(this RectTransform transform, float top) {
            transform.offsetMax = new Vector2(transform.offsetMax.x, -top);
        }

        /// <summary>
        ///     Gets the top offset of the RectTransform.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <returns>The top offset value.</returns>
        public static float GetTop(this RectTransform transform) {
            return -transform.offsetMax.y;
        }

        /// <summary>
        ///     Sets the bottom offset of the RectTransform.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="bottom">The bottom offset value.</param>
        public static void SetBottom(this RectTransform transform, float bottom) {
            transform.offsetMin = new Vector2(transform.offsetMin.x, bottom);
        }

        /// <summary>
        ///     Gets the bottom offset of the RectTransform.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <returns>The bottom offset value.</returns>
        public static float GetBottom(this RectTransform transform) {
            return transform.offsetMin.y;
        }

        /// <summary>
        ///     Sets all four edge offsets (left, top, right, bottom) at once.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="left">The left offset value.</param>
        /// <param name="top">The top offset value.</param>
        /// <param name="right">The right offset value.</param>
        /// <param name="bottom">The bottom offset value.</param>
        public static void SetLeftTopRightBottom(this RectTransform transform, float left, float top, float right,
            float bottom) {
            transform.offsetMin = new Vector2(left, bottom);
            transform.offsetMax = new Vector2(-right, -top);
        }

        #endregion

        #region PosX, PosY, Width, Height

        /// <summary>
        ///     Sets the anchored position along the X axis.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="posX">The X position value.</param>
        public static void SetPosX(this RectTransform transform, float posX) {
            transform.anchoredPosition = new Vector2(posX, transform.anchoredPosition.y);
        }

        /// <summary>
        ///     Sets the anchored position along the Y axis.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="posY">The Y position value.</param>
        public static void SetPosY(this RectTransform transform, float posY) {
            transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, posY);
        }

        /// <summary>
        ///     Sets the anchored position along both X and Y axes.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="posX">The X position value.</param>
        /// <param name="posY">The Y position value.</param>
        public static void SetPosXY(this RectTransform transform, float posX, float posY) {
            transform.anchoredPosition = new Vector2(posX, posY);
        }

        /// <summary>
        ///     Sets the width component of <see cref="RectTransform.sizeDelta"/>.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="width">The width value.</param>
        public static void SetWidth(this RectTransform transform, float width) {
            if (!Mathf.Approximately(transform.anchorMin.x, transform.anchorMax.x)) {
                var parent = transform.parent as RectTransform;
                var parentWidth = parent != null ? parent.rect.width : 0;
                width -= parentWidth * (transform.anchorMax.x - transform.anchorMin.x);
            }
            transform.sizeDelta = new Vector2(width, transform.sizeDelta.y);
        }

        /// <summary>
        ///     Gets the width of the RectTransform.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <returns>The rect width.</returns>
        public static float GetWidth(this RectTransform transform) {
            return transform.rect.width;
        }

        /// <summary>
        ///     Sets the height component of <see cref="RectTransform.sizeDelta"/>.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="height">The height value.</param>
        public static void SetHeight(this RectTransform transform, float height) {
            if (!Mathf.Approximately(transform.anchorMin.y, transform.anchorMax.y)) {
                var parent = transform.parent as RectTransform;
                var parentHeight = parent != null ? parent.rect.height : 0;
                height -= parentHeight * (transform.anchorMax.y - transform.anchorMin.y);
            }
            transform.sizeDelta = new Vector2(transform.sizeDelta.x, height);
        }

        /// <summary>
        ///     Gets the height of the RectTransform.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <returns>The rect height.</returns>
        public static float GetHeight(this RectTransform transform) {
            return transform.rect.height;
        }

        /// <summary>
        ///     Sets both width and height via <see cref="RectTransform.sizeDelta"/>.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="width">The width value.</param>
        /// <param name="height">The height value.</param>
        public static void SetWidthHeight(this RectTransform transform, float width, float height) {
            transform.sizeDelta = new Vector2(width, height);
        }

        /// <summary>
        ///     Sets both anchored position and size in a single call.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="posX">The X position value.</param>
        /// <param name="posY">The Y position value.</param>
        /// <param name="width">The width value.</param>
        /// <param name="height">The height value.</param>
        public static void SetPosAndSize(this RectTransform transform, float posX, float posY, float width,
            float height) {
            transform.anchoredPosition = new Vector2(posX, posY);
            transform.sizeDelta = new Vector2(width, height);
        }

        #endregion

        #region Anchor Offset

        /// <summary>
        ///     Sets the left anchor minimum offset as a normalized value.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="leftPercent">The left anchor percentage (0–1).</param>
        public static void SetLeftAnchorOffset(this RectTransform transform, float leftPercent) {
            transform.anchorMin = new Vector2(leftPercent, transform.anchorMin.y);
        }

        /// <summary>
        ///     Sets the right anchor maximum offset as a normalized value from the right edge.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="rightPercent">The right anchor percentage (0–1).</param>
        public static void SetRightAnchorOffset(this RectTransform transform, float rightPercent) {
            transform.anchorMax = new Vector2(1f - rightPercent, transform.anchorMax.y);
        }

        /// <summary>
        ///     Sets the top anchor maximum offset as a normalized value from the top edge.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="topPercent">The top anchor percentage (0–1).</param>
        public static void SetTopAnchorOffset(this RectTransform transform, float topPercent) {
            transform.anchorMax = new Vector2(transform.anchorMax.x, 1f - topPercent);
        }

        /// <summary>
        ///     Sets the bottom anchor minimum offset as a normalized value.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="bottomPercent">The bottom anchor percentage (0–1).</param>
        public static void SetBottomAnchorOffset(this RectTransform transform, float bottomPercent) {
            transform.anchorMin = new Vector2(transform.anchorMin.x, bottomPercent);
        }

        /// <summary>
        ///     Sets all four anchor offsets (left, top, right, bottom) at once.
        /// </summary>
        /// <param name="transform">The target RectTransform.</param>
        /// <param name="left">The left anchor percentage (0–1).</param>
        /// <param name="top">The top anchor percentage (0–1).</param>
        /// <param name="right">The right anchor percentage (0–1).</param>
        /// <param name="bottom">The bottom anchor percentage (0–1).</param>
        public static void SetAnchorOffset(this RectTransform transform, float left, float top, float right,
            float bottom) {
            transform.anchorMin = new Vector2(left, bottom);
            transform.anchorMax = new Vector2(1f - right, 1f - top);
        }

        #endregion
    }
}
