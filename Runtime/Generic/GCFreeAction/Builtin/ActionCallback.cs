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

namespace vFrame.Core.Generic
{
    public abstract class ActionCallback : GCFreeCallback<ActionCallback, Action>
    {
        protected override Action InitialCallback() {
            return Callback;
        }

        protected abstract void OnCallback();
    }

    public abstract class ActionCallback<T> : GCFreeCallback<ActionCallback<T>, Action<T>>
    {
        protected override Action<T> InitialCallback() {
            return OnCallback;
        }

        protected abstract void OnCallback(T arg);
    }

    public abstract class ActionCallback<T1, T2> : GCFreeCallback<ActionCallback<T1, T2>, Action<T1, T2>>
    {
        protected override Action<T1, T2> InitialCallback() {
            return OnCallback;
        }

        protected abstract void OnCallback(T1 arg1, T2 arg2);
    }

    public abstract class ActionCallback<T1, T2, T3> : GCFreeCallback<ActionCallback<T1, T2, T3>, Action<T1, T2, T3>>
    {
        protected override Action<T1, T2, T3> InitialCallback() {
            return OnCallback;
        }

        protected abstract void OnCallback(T1 arg, T2 arg2, T3 arg3);
    }
}