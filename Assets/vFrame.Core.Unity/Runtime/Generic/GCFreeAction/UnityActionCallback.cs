// ------------------------------------------------------------
//         File: UnityActionCallback.cs
//        Brief: GC-free UnityAction callback wrappers that cache
//               delegates to avoid per-frame allocations.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-20 16:03:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using UnityEngine.Events;
using vFrame.Core;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// GC-free callback wrapper for <see cref="UnityAction"/> delegates
    /// without arguments.
    /// </summary>
    /// <typeparam name="TC">The concrete owner type deriving from <see cref="BaseObject{IObjectPoolManager}"/>.</typeparam>
    public abstract class UnityActionCallback<TC> : GCFreeCallback<TC, UnityAction> where TC : BaseObject<IObjectPoolManager>
    {
        /// <summary>
        /// Creates and returns the cached <see cref="UnityAction"/> delegate.
        /// </summary>
        /// <returns>The cached callback delegate.</returns>
        protected override UnityAction InitialCallback() {
            return OnCallbackInternal;
        }

        /// <summary>
        /// Internal trampoline that invokes the user callback and
        /// optionally destroys the object on completion.
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
        /// Override to implement the actual callback logic.
        /// </summary>
        protected abstract void OnCallback();
    }

    /// <summary>
    /// GC-free callback wrapper for <see cref="UnityAction{TArg1}"/> delegates
    /// with one argument.
    /// </summary>
    /// <typeparam name="TC">The concrete owner type deriving from <see cref="BaseObject{IObjectPoolManager}"/>.</typeparam>
    /// <typeparam name="TArg1">The type of the first callback argument.</typeparam>
    public abstract class UnityActionCallback<TC, TArg1> : GCFreeCallback<TC, UnityAction<TArg1>> where TC : BaseObject<IObjectPoolManager>
    {
        /// <summary>
        /// Creates and returns the cached <see cref="UnityAction{TArg1}"/> delegate.
        /// </summary>
        /// <returns>The cached callback delegate.</returns>
        protected override UnityAction<TArg1> InitialCallback() {
            return OnCallbackInternal;
        }

        /// <summary>
        /// Internal trampoline that invokes the user callback and
        /// optionally destroys the object on completion.
        /// </summary>
        /// <param name="arg1">The first argument passed through from the delegate invocation.</param>
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
        /// Override to implement the actual callback logic.
        /// </summary>
        /// <param name="arg1">The first argument passed through from the delegate invocation.</param>
        protected abstract void OnCallback(TArg1 arg1);
    }

    /// <summary>
    /// GC-free callback wrapper for <see cref="UnityAction{TArg1,TArg2}"/> delegates
    /// with two arguments.
    /// </summary>
    /// <typeparam name="TC">The concrete owner type deriving from <see cref="BaseObject{IObjectPoolManager}"/>.</typeparam>
    /// <typeparam name="TArg1">The type of the first callback argument.</typeparam>
    /// <typeparam name="TArg2">The type of the second callback argument.</typeparam>
    public abstract class UnityActionCallback<TC, TArg1, TArg2> : GCFreeCallback<TC, UnityAction<TArg1, TArg2>> where TC : BaseObject<IObjectPoolManager>
    {
        /// <summary>
        /// Creates and returns the cached <see cref="UnityAction{TArg1,TArg2}"/> delegate.
        /// </summary>
        /// <returns>The cached callback delegate.</returns>
        protected override UnityAction<TArg1, TArg2> InitialCallback() {
            return OnCallbackInternal;
        }

        /// <summary>
        /// Internal trampoline that invokes the user callback and
        /// optionally destroys the object on completion.
        /// </summary>
        /// <param name="arg1">The first argument passed through from the delegate invocation.</param>
        /// <param name="arg2">The second argument passed through from the delegate invocation.</param>
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
        /// Override to implement the actual callback logic.
        /// </summary>
        /// <param name="arg1">The first argument passed through from the delegate invocation.</param>
        /// <param name="arg2">The second argument passed through from the delegate invocation.</param>
        protected abstract void OnCallback(TArg1 arg1, TArg2 arg2);
    }
}
