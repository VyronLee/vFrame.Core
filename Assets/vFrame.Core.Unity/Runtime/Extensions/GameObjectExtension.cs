// ------------------------------------------------------------
//         File: GameObjectExtension.cs
//        Brief: Extension methods for GameObject and Unity Object utilities.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-01-04 17:20:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace vFrame.Core.Unity
{
    public static class GameObjectExtension
    {
        /// <summary>
        ///     Sets the layer of the GameObject and all its children recursively.
        /// </summary>
        /// <param name="go">The GameObject whose layer to set.</param>
        /// <param name="layer">The layer index to assign.</param>
        public static void SetLayerRecursive(this GameObject go, int layer) {
            var stack = new Stack<GameObject>();
            stack.Push(go);
            while (stack.Count > 0) {
                var current = stack.Pop();
                current.layer = layer;
                for (int i = current.transform.childCount - 1; i >= 0; i--) {
                    stack.Push(current.transform.GetChild(i).gameObject);
                }
            }
        }

        /// <summary>
        ///     Sets the tag of the GameObject and all its children recursively.
        /// </summary>
        /// <param name="go">The GameObject whose tag to set.</param>
        /// <param name="tag">The tag string to assign.</param>
        public static void SetTagRecursive(this GameObject go, string tag) {
            var stack = new Stack<GameObject>();
            stack.Push(go);
            while (stack.Count > 0) {
                var current = stack.Pop();
                current.tag = tag;
                for (int i = current.transform.childCount - 1; i >= 0; i--) {
                    stack.Push(current.transform.GetChild(i).gameObject);
                }
            }
        }

        /// <summary>
        ///     Gets an existing component of the specified type, or adds one if it does not exist.
        /// </summary>
        /// <typeparam name="T">The MonoBehaviour type to get or add.</typeparam>
        /// <param name="go">The GameObject to search on.</param>
        /// <returns>The existing or newly added component instance.</returns>
        public static T GetOrAddComponent<T>(this GameObject go) where T : MonoBehaviour {
            var comp = go.GetComponent<T>();
            if (null == comp) {
                comp = go.AddComponent<T>();
            }
            return comp;
        }

        /// <summary>
        ///     Destroys a Unity Object safely, using <see cref="UnityEngine.Object.DestroyImmediate"/> in edit mode
        ///     and <see cref="Object.Destroy"/> at runtime.
        /// </summary>
        /// <param name="go">The Object to destroy.</param>
        public static void DestroyEx(this UnityEngine.Object go) {
            if (Application.isPlaying) {
                UnityEngine.Object.Destroy(go);
                return;
            }
            UnityEngine.Object.DestroyImmediate(go);
        }

        /// <summary>
        ///     Marks a Unity Object so it is not destroyed when loading a new scene,
        ///     and returns the object for fluent chaining.
        /// </summary>
        /// <typeparam name="T">The Object type.</typeparam>
        /// <param name="obj">The Object to protect from scene unload.</param>
        /// <returns>The same object instance.</returns>
        public static T DontDestroyEx<T>(this T obj) where T: UnityEngine.Object {
            if (obj) {
                UnityEngine.Object.DontDestroyOnLoad(obj);
            }
            return obj;
        }

        /// <summary>
        ///     Hides the Object in the hierarchy and inspector, and prevents it from being saved
        ///     in builds or the editor. Unlike <see cref="HideFlags.DontSave"/>, this does not
        ///     set <see cref="HideFlags.DontUnloadUnusedAsset"/>.
        /// </summary>
        /// <typeparam name="T">The Object type.</typeparam>
        /// <param name="obj">The Object to configure.</param>
        /// <returns>The same object instance.</returns>
        public static T DontSaveAndHideEx<T>(this T obj) where T : UnityEngine.Object {
            if (obj) {
                obj.hideFlags = HideFlags.HideInHierarchy
                                | HideFlags.HideInInspector
                                | HideFlags.DontSaveInBuild
                                | HideFlags.DontSaveInEditor;
            }
            return obj;
        }
    }
}