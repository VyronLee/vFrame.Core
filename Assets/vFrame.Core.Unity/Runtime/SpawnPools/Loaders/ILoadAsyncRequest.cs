// ------------------------------------------------------------
//         File: ILoadAsyncRequest.cs
//        Brief: Interface for an asynchronous GameObject load request.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-19 22:18:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using UnityEngine;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Represents an asynchronous request that produces a loaded <see cref="GameObject"/>.
    /// </summary>
    public interface ILoadAsyncRequest : IAsyncRequest
    {
        /// <summary>
        /// Gets the loaded <see cref="GameObject"/> once the request completes.
        /// </summary>
        GameObject GameObject { get; }
    }
}
