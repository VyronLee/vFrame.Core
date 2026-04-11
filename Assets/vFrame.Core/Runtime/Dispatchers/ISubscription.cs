// ------------------------------------------------------------
//         File: ISubscription.cs
//        Brief: Subscription handle interface
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    public interface ISubscription : IDestroyable
    {
        /// <summary>
        /// Gets the unique handle identifier for this subscription.
        /// </summary>
        uint Handle { get; }

        /// <summary>
        /// Gets the dispatch priority. Higher values are invoked first during Publish.
        /// Default is 0. Subscribers with the same priority are invoked in registration order.
        /// </summary>
        int Priority { get; }
    }
}
