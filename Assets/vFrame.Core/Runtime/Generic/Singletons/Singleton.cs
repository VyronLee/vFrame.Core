// ------------------------------------------------------------
//         File: Singleton.cs
//        Brief: Generic thread-safe singleton base class.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2016-07-28 15:19:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    public abstract class Singleton<T> : BaseObject where T : BaseObject, new()
    {
        private static volatile T _instance;

        private static readonly object _lockObject = new object();

        /// <summary>
        ///     Clears the static instance reference when this singleton is destroyed.
        /// </summary>
        protected override void OnDestroy() {
            if (_instance == this) {
                _instance = null;
            }
        }

        /// <summary>
        ///     Returns the singleton instance, creating it lazily if necessary.
        /// </summary>
        /// <returns>The singleton instance of type <typeparamref name="T" />.</returns>
        public static T Instance() {
            if (null == _instance) {
                lock (_lockObject) {
                    if (null == _instance) {
                        _instance = NewInstance();
                    }
                }
            }

            return _instance;
        }

        /// <summary>
        ///     Creates and initializes a new singleton instance.
        /// </summary>
        /// <returns>A newly created and initialized instance.</returns>
        private static T NewInstance() {
            var instance = new T();
            instance.Create();
            return instance;
        }
    }
}