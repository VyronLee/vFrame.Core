using System;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.EventDispatchers
{
    public sealed class InteractionSubscription : IInteractionSubscription, IPoolObjectResetable
    {
        public uint Handle { get; set; }
        public Type MessageType { get; set; }
        public Delegate Action { get; set; }
        public bool Destroyed { get; private set; }

        public void Destroy() {
            if (Destroyed) {
                return;
            }
            Destroyed = true;
            Action = null;
            MessageType = null;
            Handle = 0;
        }

        public void Reset() {
            Destroyed = false;
            Handle = 0;
            MessageType = null;
            Action = null;
        }
    }
}
