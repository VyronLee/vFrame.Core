using System;
using vFrame.Core.Base;

namespace vFrame.Core.EventDispatchers
{
    /// <summary>
    /// Lightweight typed interaction subscription that tears down through the shared destroy boundary.
    /// </summary>
    public sealed class InteractionSubscription : BaseObject, IInteractionSubscription
    {
        private Action<InteractionSubscription> _unsubscribe;

        public uint Handle { get; set; }
        public Type MessageType { get; set; }
        public Delegate Action { get; set; }

        /// <summary>
        /// Explicit caller-managed unsubscribe entry point for bare subscriptions.
        /// Bound subscriptions reach the same teardown through their owner or lifetime.
        /// </summary>
        public void Unsubscribe() {
            Destroy();
        }

        protected override void OnCreate() { }

        protected override void OnDestroy() {
            _unsubscribe?.Invoke(this);
            _unsubscribe = null;
            Action = null;
            MessageType = null;
            Handle = 0;
        }

        internal void SetUnsubscribe(Action<InteractionSubscription> unsubscribe) {
            _unsubscribe = unsubscribe;
        }
    }
}
