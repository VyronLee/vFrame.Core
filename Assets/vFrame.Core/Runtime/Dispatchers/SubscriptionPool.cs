using vFrame.Core.ObjectPools;

namespace vFrame.Core.Dispatchers
{
    internal class SubscriptionPool : ObjectPool<Subscription, SubscriptionAllocator> { }

    internal class SubscriptionAllocator : IPoolObjectAllocator<Subscription>
    {
        public Subscription Alloc() {
            return new Subscription();
        }

        public void Reset(Subscription obj) {
            obj.Reset();
        }
    }
}
