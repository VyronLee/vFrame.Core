// ------------------------------------------------------------
//         File: RecycleOnDestroy.cs
//        Brief: RecycleOnDestroy.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-20 15:57
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using vFrame.Core.Base;
using vFrame.Core.Exceptions;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.Generic
{
    public abstract class RecycleOnDestroy<T> : CreateAbility<T, IObjectPoolManager> where T : BaseObject<IObjectPoolManager>
    {
        protected IObjectPoolManager PoolManager { get; set; }

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