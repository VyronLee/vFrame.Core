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
    public abstract class GCFreeCallback<TC, TCallback> : RecycleOnDestroy<TC>
        where TC : BaseObject<IObjectPoolManager>
        where TCallback : Delegate
    {
        protected bool AutoDestroyOnCallback { get; set; } = true;

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

        public static implicit operator TCallback(GCFreeCallback<TC, TCallback> callback) {
            return callback.Callback;
        }
    }
}