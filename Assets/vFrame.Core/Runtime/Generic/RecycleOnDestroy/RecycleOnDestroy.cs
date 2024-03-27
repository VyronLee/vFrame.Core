// ------------------------------------------------------------
//         File: RecycleOnDestroy.cs
//        Brief: RecycleOnDestroy.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-20 15:57
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using vFrame.Core.Base;
using vFrame.Core.Exceptions;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.Generic
{
    public abstract class RecycleOnDestroy<TC> : BaseObject<IObjectPoolManager> where TC : BaseObject<IObjectPoolManager>
    {
        protected IObjectPoolManager PoolManager { get; set; }

        public static TC CreateWithSharedPools() {
            var ret = Activator.CreateInstance<TC>();
            ret.Create(ObjectPoolManager.Shared);
            return ret;
        }

        protected override void OnCreate(IObjectPoolManager manager) {
            ThrowHelper.ThrowIfNull(manager, nameof(manager));
            PoolManager = manager;
        }

        protected override void OnDestroy() {
            PoolManager?.Return(this);
            PoolManager = null;
        }

    }
}