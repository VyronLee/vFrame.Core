// ------------------------------------------------------------
//         File: SpawnPoolsSettings.cs
//        Brief: Configuration settings for spawn pools including
//                capacity, lifetime, GC interval, and diagnostics.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 23:47:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using UnityEngine;
using vFrame.Core;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Holds configurable parameters that govern spawn pool behavior such as
    /// capacity limits, object lifetime, garbage-collection intervals, and diagnostics.
    /// </summary>
    public class SpawnPoolsSettings
    {
        /// <summary>
        /// Shared log tag used by spawn pools for diagnostic output.
        /// </summary>
        public static LogTag LogTag = new LogTag("SpawnPools");

        /// <summary>
        /// Gets or sets the maximum number of retained pools. When exceeded,
        /// least-used pools are evicted during the GC cycle.
        /// </summary>
        public int Capacity { get; set; } = 40;

        /// <summary>
        /// Gets or sets the inactive lifetime threshold in frames before a pool is
        /// considered timed out and eligible for cleanup.
        /// </summary>
        public int LifeTime { get; set; } = 30 * 60 * 5;

        /// <summary>
        /// Gets or sets the interval in frames between automatic pool garbage-collection passes.
        /// </summary>
        public int GCInterval { get; set; } = 600;

        /// <summary>
        /// Gets or sets whether spawn pool diagnostics logging is enabled.
        /// </summary>
        public bool EnableDiagnostics { get; set; }

        /// <summary>
        /// Gets or sets the world-space position assigned to the root container
        /// so that inactive pooled objects are hidden from view.
        /// </summary>
        public Vector3 RootPosition { get; set; } = new Vector3(-1000, -1000, -1000);

        /// <summary>
        /// Gets a singleton instance with default settings.
        /// </summary>
        public static SpawnPoolsSettings Default { get; } = new SpawnPoolsSettings();
    }
}
