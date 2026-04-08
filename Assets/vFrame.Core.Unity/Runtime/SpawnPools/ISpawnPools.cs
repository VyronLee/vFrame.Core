//------------------------------------------------------------
//        File:  ISpawnPools.cs
//       Brief:  Spawn pools interface.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-08 23:47
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using UnityEngine;

namespace vFrame.Core.Unity.SpawnPools
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
        GameObject Spawn(string assetPath, Transform parent = null);

        /// <summary>
        /// Starts an async instance acquire flow that still resolves to pooled instance reuse semantics.
        /// Call <see cref="Update"/> until the request completes.
        /// </summary>
        ILoadAsyncRequest SpawnAsync(string assetPath, Transform parent = null);

        /// <summary>
        /// Ends the current usage cycle for a spawned instance and returns it to its matching pool.
        /// </summary>
        void Recycle(GameObject obj);

        /// <summary>
        /// Warms pool state for the given asset paths without turning <see cref="ISpawnPools"/>
        /// into a broader resource ownership runtime.
        /// </summary>
        IPreloadAsyncRequest PreloadAsync(string[] assetPaths);
    }
}
