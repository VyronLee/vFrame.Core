// ------------------------------------------------------------
//         File: ISpawnPools.cs
//        Brief: Contract for the Unity-side spawn pools runtime,
//                defining spawn, recycle, preload, and update
//                operations for pooled instance management.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 23:47:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using UnityEngine;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Unity-side instance reuse contract for prefab and <see cref="GameObject"/> pooling.
    /// This surface intentionally manages spawned instance reuse only; resource location,
    /// download, patching, and dependency ownership stay outside of <see cref="ISpawnPools"/>.
    /// </summary>
    public interface ISpawnPools
    {
        /// <summary>
        /// Advances background async requests and scheduled pool cleanup work.
        /// </summary>
        void Update();

        /// <summary>
        /// Gets a pooled or newly loaded instance for immediate use.
        /// </summary>
        /// <param name="assetPath">Asset path identifying the prefab to spawn.</param>
        /// <param name="parent">Optional parent transform; defaults to the pool root.</param>
        /// <returns>A spawned <see cref="GameObject"/> ready for use.</returns>
        GameObject Spawn(string assetPath, Transform parent = null);

        /// <summary>
        /// Starts an async instance acquire flow that still resolves to pooled instance reuse semantics.
        /// Call <see cref="Update"/> until the request completes.
        /// </summary>
        /// <param name="assetPath">Asset path identifying the prefab to spawn.</param>
        /// <param name="parent">Optional parent transform; defaults to the pool root.</param>
        /// <returns>An <see cref="ILoadAsyncRequest"/> that delivers the object on completion.</returns>
        ILoadAsyncRequest SpawnAsync(string assetPath, Transform parent = null);

        /// <summary>
        /// Ends the current usage cycle for a spawned instance and returns it to its matching pool.
        /// </summary>
        /// <param name="obj">The spawned object to recycle.</param>
        void Recycle(GameObject obj);

        /// <summary>
        /// Warms pool state for the given asset paths without turning <see cref="ISpawnPools"/>
        /// into a broader resource ownership runtime.
        /// </summary>
        /// <param name="assetPaths">Array of asset paths to preload into pools.</param>
        /// <returns>An <see cref="IPreloadAsyncRequest"/> that completes when preloading is done.</returns>
        IPreloadAsyncRequest PreloadAsync(string[] assetPaths);
    }
}
