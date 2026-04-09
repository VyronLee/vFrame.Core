// ------------------------------------------------------------
//         File: IGameObjectLoader.cs
//        Brief: Interface for synchronous and asynchronous GameObject loading.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-02-18 14:49:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using UnityEngine;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Provides synchronous and asynchronous methods for loading a <see cref="GameObject"/>.
    /// </summary>
    public interface IGameObjectLoader
    {
        /// <summary>
        /// Loads the <see cref="GameObject"/> synchronously.
        /// </summary>
        /// <returns>The loaded <see cref="GameObject"/>.</returns>
        GameObject Load();

        /// <summary>
        /// Starts an asynchronous load operation.
        /// </summary>
        /// <returns>A <see cref="LoadAsyncRequest"/> that completes when the load finishes.</returns>
        LoadAsyncRequest LoadAsync();
    }
}
