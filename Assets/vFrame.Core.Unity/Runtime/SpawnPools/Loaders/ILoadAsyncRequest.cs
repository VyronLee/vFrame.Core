// ------------------------------------------------------------
//         File: ILoaderAsyncRequest.cs
//        Brief: ILoaderAsyncRequest.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-19 22:18
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using UnityEngine;
using vFrame.Core.Unity.Asynchronous;

namespace vFrame.Core.Unity.SpawnPools
{
    public interface ILoadAsyncRequest : IAsyncRequest
    {
        GameObject GameObject { get; }
    }
}