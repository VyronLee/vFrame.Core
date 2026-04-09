// ------------------------------------------------------------
//         File: IPool.cs
//        Brief: Internal contract for a single asset-path pool
//                managing spawn, async spawn, and recycle of
//                Unity GameObject instances.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 23:48:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using UnityEngine;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Internal interface for a single asset-path object pool.
    /// Defines the core spawn, async spawn, recycle, and count operations.
    /// </summary>
    internal interface IPool
    {
        /// <summary>
        /// Gets the number of inactive objects currently retained in this pool.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Spawns a pooled or newly created instance under the specified parent.
        /// </summary>
        /// <param name="parent">Optional parent transform; defaults to the pool root.</param>
        /// <returns>A <see cref="GameObject"/> ready for use.</returns>
        GameObject Spawn(Transform parent = null);

        /// <summary>
        /// Starts an asynchronous spawn operation for this pool.
        /// </summary>
        /// <param name="parent">Optional parent transform; defaults to the pool root.</param>
        /// <returns>An <see cref="ILoadAsyncRequest"/> that delivers the object on completion.</returns>
        ILoadAsyncRequest SpawnAsync(Transform parent = null);

        /// <summary>
        /// Returns a previously spawned object to this pool for future reuse.
        /// </summary>
        /// <param name="obj">The object to recycle.</param>
        void Recycle(GameObject obj);
    }
}
