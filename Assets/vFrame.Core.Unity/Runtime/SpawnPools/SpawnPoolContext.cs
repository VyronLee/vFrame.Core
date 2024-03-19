// ------------------------------------------------------------
//         File: SpawnPoolContext.cs
//        Brief: SpawnPoolContext.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-19 23:22
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using UnityEngine;

namespace vFrame.Core.Unity.SpawnPools
{
    internal class SpawnPoolContext
    {
        public Transform Parent { get; set; }
        public SpawnPoolsSettings Settings { get; set; }
    }
}