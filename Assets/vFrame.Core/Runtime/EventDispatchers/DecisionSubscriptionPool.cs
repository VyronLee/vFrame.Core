using vFrame.Core.ObjectPools;

namespace vFrame.Core.EventDispatchers
{
    internal class DecisionSubscriptionPool : ObjectPool<DecisionSubscription, DecisionSubscriptionAllocator> { }

    internal class DecisionSubscriptionAllocator : IPoolObjectAllocator<DecisionSubscription>
    {
        public DecisionSubscription Alloc() {
            return new DecisionSubscription();
        }

        public void Reset(DecisionSubscription obj) {
            obj.Reset();
        }
    }
}
