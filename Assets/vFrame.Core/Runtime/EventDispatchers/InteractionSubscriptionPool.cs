using vFrame.Core.ObjectPools;

namespace vFrame.Core.EventDispatchers
{
    internal class InteractionSubscriptionPool : ObjectPool<InteractionSubscription, InteractionSubscriptionAllocator> { }

    internal class InteractionSubscriptionAllocator : IPoolObjectAllocator<InteractionSubscription>
    {
        public InteractionSubscription Alloc() {
            return new InteractionSubscription();
        }

        public void Reset(InteractionSubscription obj) {
            obj.Reset();
        }
    }
}
