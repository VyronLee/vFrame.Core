// ------------------------------------------------------------
//         File: RecycleOnDestroy.cs
//        Brief: Base class that returns itself to an object pool on destroy.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-20 15:57:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using vFrame.Core;

namespace vFrame.Core
{
    public abstract class RecycleOnDestroy<TC> : BaseObject<IObjectPoolManager> where TC : BaseObject<IObjectPoolManager>
    {
        protected IObjectPoolManager PoolManager { get; set; }

        /// <summary>
        ///     Creates an instance using the shared object pool manager.
        /// </summary>
        /// <returns>A new instance backed by the shared pool manager.</returns>
        public static TC CreateWithSharedPools() {
            var ret = Activator.CreateInstance<TC>();
            ret.Create(ObjectPoolManager.Shared);
            return ret;
        }

        /// <summary>
        ///     Stores the pool manager reference during creation.
        /// </summary>
        /// <param name="manager">The pool manager that owns this instance.</param>
        protected override void OnCreate(IObjectPoolManager manager) {
            ThrowHelper.ThrowIfNull(manager, nameof(manager));
            PoolManager = manager;
        }

        /// <summary>
        ///     Returns this instance to the pool and clears the manager reference.
        /// </summary>
        protected override void OnDestroy() {
            PoolManager?.Return(this);
            PoolManager = null;
        }

    }
}
