// ------------------------------------------------------------
//         File: SubscriptionPool.cs
//        Brief: Subscription object pool and allocator
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================


namespace vFrame.Core
{
    internal class SubscriptionPool : ObjectPool<Subscription, SubscriptionAllocator>
    { }

    internal class SubscriptionAllocator : IPoolObjectAllocator<Subscription>
    {
        /// <summary>
        ///     Allocates a new <see cref="Subscription" /> instance.
        /// </summary>
        /// <returns>The newly created subscription instance.</returns>
        public Subscription Alloc() {
            return new Subscription();
        }

        /// <summary>
        ///     Resets the subscription instance so it can be reused by the object pool.
        /// </summary>
        /// <param name="obj">The subscription instance to reset.</param>
        public void Reset(Subscription obj) {
            obj.Reset();
        }
    }
}