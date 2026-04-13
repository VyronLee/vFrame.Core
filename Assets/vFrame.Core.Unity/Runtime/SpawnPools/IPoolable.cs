// ------------------------------------------------------------
//         File: IPoolable.cs
//        Brief: Interface for components that need lifecycle
//                callbacks when spawned or recycled by a pool.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-04-12 10:25:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Interface for components that require notification when a pooled
    /// <see cref="GameObject"/> is spawned or recycled.
    /// Implement this on any <see cref="UnityEngine.MonoBehaviour"/> attached
    /// to a pooled prefab to receive lifecycle callbacks.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Called when the object is returned to the pool via <c>Recycle</c>.
        /// Use this to reset state, cancel coroutines, or release transient resources.
        /// </summary>
        void OnRecycled();

        /// <summary>
        /// Called when the object is acquired from the pool via <c>Spawn</c>.
        /// Use this to re-initialize state or start coroutines.
        /// </summary>
        void OnSpawned();
    }
}
