using System;
using vFrame.Core.Base;

namespace vFrame.Core.EventDispatchers
{
    /// <summary>
    /// Retained typed interaction surface for new core publish-subscribe work.
    /// Bare subscriptions stay caller-managed, while owner-bound and lifetime-bound
    /// subscriptions end automatically with their lifecycle boundary.
    /// </summary>
    public interface IInteractionDispatcher
    {
        /// <summary>
        /// Creates a bare typed subscription that remains active until explicitly unsubscribed.
        /// </summary>
        IInteractionSubscription Subscribe<TMessage>(Action<TMessage> action) where TMessage : class;

        /// <summary>
        /// Creates an owner-bound typed subscription that ends when the owner is destroyed.
        /// </summary>
        IInteractionSubscription Subscribe<TMessage>(Action<TMessage> action, BaseObject owner) where TMessage : class;

        /// <summary>
        /// Creates a lifetime-bound typed subscription that ends when the lifetime is destroyed.
        /// </summary>
        IInteractionSubscription Subscribe<TMessage>(Action<TMessage> action, ILifetime lifetime) where TMessage : class;

        /// <summary>
        /// Explicitly ends a typed subscription.
        /// </summary>
        void Unsubscribe(IInteractionSubscription subscription);

        /// <summary>
        /// Publishes a typed message through the retained primary interaction path.
        /// </summary>
        void Publish<TMessage>(TMessage message) where TMessage : class;

        /// <summary>
        /// Returns the current number of tracked typed subscriptions.
        /// </summary>
        int GetInteractionSubscriptionCount();
    }
}
