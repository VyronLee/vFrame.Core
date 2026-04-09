// ------------------------------------------------------------
//         File: IDispatcher.cs
//        Brief: Unified dispatcher interface aggregating event, command, request and decision dispatching
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    public interface IDispatcher : IEventDispatcher, ICommandDispatcher, IRequestDispatcher, IDecisionDispatcher
    {
        /// <summary>
        /// Removes all registered subscriptions.
        /// </summary>
        void RemoveAllSubscriptions();

        /// <summary>
        /// Gets the total number of subscriptions across all types.
        /// </summary>
        /// <returns>The total subscription count.</returns>
        int GetTotalSubscriptionCount();
    }
}
