// ------------------------------------------------------------
//         File: UnityActionCallback.cs
//        Brief: UnityActionCallback.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-20 16:3
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using UnityEngine.Events;
using vFrame.Core.Base;
using vFrame.Core.Generic;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.Unity.Generic
{
    public abstract class UnityActionCallback<TC> : GCFreeCallback<TC, UnityAction> where TC : BaseObject<IObjectPoolManager>
    {
        protected override UnityAction InitialCallback() {
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

    public abstract class UnityActionCallback<TC, TArg1> : GCFreeCallback<TC, UnityAction<TArg1>> where TC : BaseObject<IObjectPoolManager>
    {
        protected override UnityAction<TArg1> InitialCallback() {
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

    public abstract class UnityActionCallback<TC, TArg1, TArg2> : GCFreeCallback<TC, UnityAction<TArg1, TArg2>> where TC : BaseObject<IObjectPoolManager>
    {
        protected override UnityAction<TArg1, TArg2> InitialCallback() {
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