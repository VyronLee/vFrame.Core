// ------------------------------------------------------------
//         File: SpawnPoolsContext.cs
//        Brief: Lightweight Unity-side context shared by retained
//                instance pools; holds pool parenting and settings.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-19 23:22:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using UnityEngine;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Lightweight Unity-side context shared by retained instance pools.
    /// Keeps pool parenting and settings local to the SpawnPools runtime layer.
    /// </summary>
    internal class SpawnPoolsContext
    {
        /// <summary>
        /// Gets or sets the parent transform under which pooled objects are organized.
        /// </summary>
        public Transform Parent { get; set; }

        /// <summary>
        /// Gets or sets the spawn pool settings driving capacity, lifetime, and GC behavior.
        /// </summary>
        public SpawnPoolsSettings Settings { get; set; }
    }
}
