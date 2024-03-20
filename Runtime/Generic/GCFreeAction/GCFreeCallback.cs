// ------------------------------------------------------------
//         File: GCFreeCallback.cs
//        Brief: GCFreeCallback.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-20 15:45
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using vFrame.Core.Base;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.Generic
{
    public abstract class GCFreeCallback<T, TCallback> : RecycleOnDestroy<T>
        where T : BaseObject<IObjectPoolManager>
        where TCallback : Delegate
    {
        public static T CreateWithSharedPools() {
            var ret = Activator.CreateInstance<T>();
            ret.Create(ObjectPoolManager.Shared);
            return ret;
        }

        public TCallback Callback { get; private set; }

        protected abstract TCallback InitialCallback();

        protected override void OnCreate(IObjectPoolManager manager) {
            base.OnCreate(manager);
            Callback = InitialCallback();
        }

        protected override void OnDestroy() {
            Callback = null;
            base.OnDestroy();
        }
    }
}