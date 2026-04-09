// ------------------------------------------------------------
//         File: GCFreeCallback.cs
//        Brief: GC-free callback base that recycles itself via object pooling.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-20 15:45:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using vFrame.Core;

namespace vFrame.Core
{
    public abstract class GCFreeCallback<TC, TCallback> : RecycleOnDestroy<TC>
        where TC : BaseObject<IObjectPoolManager>
        where TCallback : Delegate
    {
        protected bool AutoDestroyOnCallback { get; set; } = true;

        public TCallback Callback { get; private set; }

        /// <summary>
        ///     Creates the initial callback delegate for this instance.
        /// </summary>
        /// <returns>The callback delegate to expose.</returns>
        protected abstract TCallback InitialCallback();

        /// <summary>
        ///     Initializes the callback during creation.
        /// </summary>
        /// <param name="manager">The pool manager that owns this instance.</param>
        protected override void OnCreate(IObjectPoolManager manager) {
            base.OnCreate(manager);
            Callback = InitialCallback();
        }

        /// <summary>
        ///     Clears the callback reference and invokes base destruction.
        /// </summary>
        protected override void OnDestroy() {
            Callback = null;
            base.OnDestroy();
        }

        /// <summary>
        ///     Implicitly converts to the underlying callback delegate.
        /// </summary>
        /// <param name="callback">The callback wrapper instance.</param>
        public static implicit operator TCallback(GCFreeCallback<TC, TCallback> callback) {
            return callback.Callback;
        }
    }
}
