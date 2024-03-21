// ------------------------------------------------------------
//         File: ActionCallback.cs
//        Brief: ActionCallback.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-20 16:3
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using vFrame.Core.Base;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.Generic
{
    public abstract class ActionCallback<TC> : GCFreeCallback<TC, Action> where TC : BaseObject<IObjectPoolManager>
    {
        protected override Action InitialCallback() {
            return OnCallbackInternal;
        }

        private void OnCallbackInternal() {
            try {
                OnCallback();
            }
            finally {
                if (AutoDestroyOnCallback) {
                    Destroy();
                }
            }
        }

        protected abstract void OnCallback();
    }

    public abstract class ActionCallback<TC, TArg1> : GCFreeCallback<TC, Action<TArg1>> where TC : BaseObject<IObjectPoolManager>
    {
        protected override Action<TArg1> InitialCallback() {
            return OnCallbackInternal;
        }

        private void OnCallbackInternal(TArg1 arg1) {
            try {
                OnCallback(arg1);
            }
            finally {
                if (AutoDestroyOnCallback) {
                    Destroy();
                }
            }
        }

        protected abstract void OnCallback(TArg1 arg1);
    }

    public abstract class ActionCallback<TC, TArg1, TArg2> : GCFreeCallback<TC, Action<TArg1, TArg2>> where TC : BaseObject<IObjectPoolManager>
    {
        protected override Action<TArg1, TArg2> InitialCallback() {
            return OnCallbackInternal;
        }

        private void OnCallbackInternal(TArg1 arg1, TArg2 arg2) {
            try {
                OnCallback(arg1, arg2);
            }
            finally {
                if (AutoDestroyOnCallback) {
                    Destroy();
                }
            }
        }

        protected abstract void OnCallback(TArg1 arg1, TArg2 arg2);
    }
}