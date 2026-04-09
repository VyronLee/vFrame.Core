// ------------------------------------------------------------
//         File: GameObjectUtils.cs
//        Brief: Utility class for GameObject creation and hierarchy operations.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using UnityEngine;

namespace vFrame.Core.Unity
{
    public static class GameObjectUtils
    {
        /// <summary>
        ///     Creates a GameObject hierarchy from a slash-separated path.
        ///     Missing intermediate GameObjects are created automatically.
        /// </summary>
        /// <param name="fullPath">
        ///     Slash-separated path describing the hierarchy, e.g. <c>"Root/Child/Leaf"</c>.
        /// </param>
        /// <returns>
        ///     The <see cref="GameObject"/> corresponding to the last path segment,
        ///     or <c>null</c> if <paramref name="fullPath"/> is empty.
        /// </returns>
        public static GameObject RecursiveCreateGameObject(string fullPath) {
            Transform last = null;
            var paths = fullPath.Split('/');
            foreach (var path in paths) {
                if (string.IsNullOrEmpty(path)) {
                    continue;
                }

                GameObject root = null;
                if (null != last) {
                    var rootTransform = last.Find(path);
                    if (rootTransform) {
                        root = rootTransform.gameObject;
                    }
                }
                else {
                    root = GameObject.Find(path);
                }

                if (null == root) {
                    root = new GameObject(path);
                }
                root.transform.SetParent(last, false);
                last = root.transform;
            }

            return null != last ? last.gameObject : null;
        }
    }
}
