// ------------------------------------------------------------
//         File: Subscription.cs
//        Brief: Subscription handle implementation holding callback delegate and destroy state
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    public sealed class Subscription : ISubscription, IPoolObjectResetable
    {
        public uint Handle { get; set; }
        public Type MessageType { get; set; }
        public Delegate Action { get; set; }
        public bool Destroyed { get; private set; }

        /// <summary>
        /// Destroys the subscription, clearing the held callback delegate and metadata.
        /// </summary>
        public void Destroy() {
            if (Destroyed) {
                return;
            }
            Destroyed = true;
            Action = null;
            MessageType = null;
            Handle = 0;
        }

        /// <summary>
        /// Releases the subscription, equivalent to <see cref="Destroy"/>.
        /// </summary>
        public void Dispose() {
            Destroy();
        }

        /// <summary>
        /// Resets the subscription state so it can be reused by the object pool.
        /// </summary>
        public void Reset() {
            Destroyed = false;
            Handle = 0;
            MessageType = null;
            Action = null;
        }
    }
}
