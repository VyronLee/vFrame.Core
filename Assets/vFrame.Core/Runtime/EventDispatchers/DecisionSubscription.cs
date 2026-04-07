using System;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.EventDispatchers
{
    public sealed class DecisionSubscription : IDecisionSubscription, IPoolObjectResetable
    {
        public uint Handle { get; set; }
        public Type DecisionType { get; set; }
        public Delegate Handler { get; set; }
        public bool Destroyed { get; private set; }

        public void Destroy() {
            if (Destroyed) {
                return;
            }
            Destroyed = true;
            Handler = null;
            DecisionType = null;
            Handle = 0;
        }

        public void Reset() {
            Destroyed = false;
            Handle = 0;
            DecisionType = null;
            Handler = null;
        }
    }
}
