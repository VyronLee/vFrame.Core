// ------------------------------------------------------------
//         File: MonoSingleton.cs
//        Brief: Generic MonoBehaviour-based singleton that lazily
//               creates a GameObject and ensures only one instance
//               exists at a time.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 23:37:00
//    Copyright: Copyright (c) 2019, VyronLee
// ============================================================

using UnityEngine;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Provides a singleton pattern for <see cref="MonoBehaviour"/> types.
    /// The instance is lazily created on first access by instantiating a new
    /// <see cref="GameObject"/> and attaching the component.
    /// </summary>
    /// <typeparam name="T">The concrete MonoBehaviour type.</typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _instanceCreated;

        /// <summary>
        /// Gets the singleton instance, creating it on first access.
        /// </summary>
        public static T Instance {
            get {
                if (_instanceCreated) {
                    return _instance;
                }
                _instance = new GameObject(typeof(T).Name).AddComponent<T>();
                _instanceCreated = true;
                return _instance;
            }
        }

        /// <summary>
        /// Ensures only one instance exists. If another instance already exists,
        /// this duplicate is destroyed; otherwise this instance is registered.
        /// </summary>
        protected virtual void Awake() {
            if (_instance != null && _instance != this as T) {
                Destroy(this);
                return;
            }
            _instance = this as T;
            _instanceCreated = true;
        }

        /// <summary>
        /// Clears the static references when the active instance is destroyed.
        /// </summary>
        protected void OnDestroy() {
            if (_instance != this as T) {
                return;
            }
            _instance = null;
            _instanceCreated = false;
        }
    }
}
