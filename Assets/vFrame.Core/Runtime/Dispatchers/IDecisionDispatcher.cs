// ------------------------------------------------------------
//         File: IDecisionDispatcher.cs
//        Brief: Decision dispatcher interface
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    public interface IDecisionDispatcher
    {
        /// <summary>
        /// Listens for decisions of the specified type with default priority (0).
        /// </summary>
        /// <param name="handler">The decision handler callback; returns <c>true</c> to approve, <c>false</c> to veto.</param>
        /// <typeparam name="TDecision">The decision type.</typeparam>
        /// <returns>A subscription handle.</returns>
        ISubscription Listen<TDecision>(Func<TDecision, bool> handler)
            where TDecision : IDecision;

        /// <summary>
        /// Listens for decisions of the specified type, bound to the owner's lifetime.
        /// </summary>
        /// <param name="handler">The decision handler callback; returns <c>true</c> to approve, <c>false</c> to veto.</param>
        /// <param name="owner">The subscription owner; the subscription is automatically cancelled when the owner is destroyed.</param>
        /// <typeparam name="TDecision">The decision type.</typeparam>
        /// <returns>A subscription handle.</returns>
        ISubscription Listen<TDecision>(Func<TDecision, bool> handler, BaseObject owner)
            where TDecision : IDecision;

        /// <summary>
        /// Listens for decisions of the specified type, bound to the given lifetime.
        /// </summary>
        /// <param name="handler">The decision handler callback; returns <c>true</c> to approve, <c>false</c> to veto.</param>
        /// <param name="lifetime">The lifetime boundary; the subscription is automatically cancelled when the lifetime ends.</param>
        /// <typeparam name="TDecision">The decision type.</typeparam>
        /// <returns>A subscription handle.</returns>
        ISubscription Listen<TDecision>(Func<TDecision, bool> handler, ILifetime lifetime)
            where TDecision : IDecision;

        /// <summary>
        /// Listens for decisions of the specified type with explicit priority.
        /// Higher priority listeners are invoked first.
        /// </summary>
        /// <param name="handler">The decision handler callback; returns <c>true</c> to approve, <c>false</c> to veto.</param>
        /// <param name="priority">The dispatch priority. Higher values are invoked first.</param>
        /// <typeparam name="TDecision">The decision type.</typeparam>
        /// <returns>A subscription handle.</returns>
        ISubscription Listen<TDecision>(Func<TDecision, bool> handler, int priority)
            where TDecision : IDecision;

        /// <summary>
        /// Listens for decisions of the specified type with explicit priority and lifetime binding.
        /// </summary>
        /// <param name="handler">The decision handler callback; returns <c>true</c> to approve, <c>false</c> to veto.</param>
        /// <param name="priority">The dispatch priority. Higher values are invoked first.</param>
        /// <param name="lifetime">The lifetime boundary; the subscription is automatically cancelled when the lifetime ends.</param>
        /// <typeparam name="TDecision">The decision type.</typeparam>
        /// <returns>A subscription handle.</returns>
        ISubscription Listen<TDecision>(Func<TDecision, bool> handler, int priority, ILifetime lifetime)
            where TDecision : IDecision;

        /// <summary>
        /// Cancels the specified decision listener subscription.
        /// </summary>
        /// <param name="subscription">The subscription handle to cancel.</param>
        void Unlisten(ISubscription subscription);

        /// <summary>
        /// Initiates a decision vote; all listeners must approve for the result to be <c>true</c>.
        /// </summary>
        /// <param name="decision">The decision payload.</param>
        /// <typeparam name="TDecision">The decision type.</typeparam>
        /// <returns><c>true</c> if all listeners approved; <c>false</c> if no listeners exist or any listener vetoed.</returns>
        bool Decide<TDecision>(in TDecision decision)
            where TDecision : IDecision;

        /// <summary>
        /// Gets the current number of decision subscriptions.
        /// </summary>
        /// <returns>The number of decision subscriptions.</returns>
        int GetDecisionSubscriptionCount();
    }
}
