// ------------------------------------------------------------
//         File: ActionCallback.cs
//        Brief: GC-free Action callbacks with zero, one, or two arguments.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-20 16:03:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    public abstract class ActionCallback<TC> : GCFreeCallback<TC, Action> where TC : BaseObject<IObjectPoolManager>
    {
        /// <summary>
        ///     Returns the initial zero-argument callback delegate.
        /// </summary>
        /// <returns>The internal callback wrapper.</returns>
        protected override Action InitialCallback() {
            return OnCallbackInternal;
        }

        /// <summary>
        ///     Invokes the callback and optionally destroys this instance.
        /// </summary>
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

        /// <summary>
        ///     Override to define the callback logic.
        /// </summary>
        protected abstract void OnCallback();
    }

    public abstract class ActionCallback<TC, TArg1> : GCFreeCallback<TC, Action<TArg1>>
        where TC : BaseObject<IObjectPoolManager>
    {
        /// <summary>
        ///     Returns the initial single-argument callback delegate.
        /// </summary>
        /// <returns>The internal callback wrapper.</returns>
        protected override Action<TArg1> InitialCallback() {
            return OnCallbackInternal;
        }

        /// <summary>
        ///     Invokes the callback and optionally destroys this instance.
        /// </summary>
        /// <param name="arg1">The first callback argument.</param>
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

        /// <summary>
        ///     Override to define the callback logic.
        /// </summary>
        /// <param name="arg1">The first argument.</param>
        protected abstract void OnCallback(TArg1 arg1);
    }

    public abstract class ActionCallback<TC, TArg1, TArg2> : GCFreeCallback<TC, Action<TArg1, TArg2>>
        where TC : BaseObject<IObjectPoolManager>
    {
        /// <summary>
        ///     Returns the initial two-argument callback delegate.
        /// </summary>
        /// <returns>The internal callback wrapper.</returns>
        protected override Action<TArg1, TArg2> InitialCallback() {
            return OnCallbackInternal;
        }

        /// <summary>
        ///     Invokes the callback and optionally destroys this instance.
        /// </summary>
        /// <param name="arg1">The first callback argument.</param>
        /// <param name="arg2">The second callback argument.</param>
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

        /// <summary>
        ///     Override to define the callback logic.
        /// </summary>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        protected abstract void OnCallback(TArg1 arg1, TArg2 arg2);
    }
}