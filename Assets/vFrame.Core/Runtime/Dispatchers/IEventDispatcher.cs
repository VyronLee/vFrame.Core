// ------------------------------------------------------------
//         File: IEventDispatcher.cs
//        Brief: Event dispatcher interface
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    public interface IEventDispatcher
    {
        /// <summary>
        ///     Subscribes to events of the specified type with default priority (0).
        /// </summary>
        /// <param name="action">The event handler callback.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <returns>A subscription handle.</returns>
        ISubscription Subscribe<TEvent>(Action<TEvent> action)
            where TEvent : IEvent;

        /// <summary>
        ///     Subscribes to events of the specified type, bound to the owner's lifetime.
        /// </summary>
        /// <param name="action">The event handler callback.</param>
        /// <param name="owner">The subscription owner; the subscription is automatically cancelled when the owner is destroyed.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <returns>A subscription handle.</returns>
        ISubscription Subscribe<TEvent>(Action<TEvent> action, BaseObject owner)
            where TEvent : IEvent;

        /// <summary>
        ///     Subscribes to events of the specified type, bound to the given lifetime.
        /// </summary>
        /// <param name="action">The event handler callback.</param>
        /// <param name="lifetime">The lifetime boundary; the subscription is automatically cancelled when the lifetime ends.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <returns>A subscription handle.</returns>
        ISubscription Subscribe<TEvent>(Action<TEvent> action, ILifetime lifetime)
            where TEvent : IEvent;

        /// <summary>
        ///     Subscribes to events of the specified type with explicit priority.
        ///     Higher priority subscribers are invoked first during Publish.
        /// </summary>
        /// <param name="action">The event handler callback.</param>
        /// <param name="priority">The dispatch priority. Higher values are invoked first.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <returns>A subscription handle.</returns>
        ISubscription Subscribe<TEvent>(Action<TEvent> action, int priority)
            where TEvent : IEvent;

        /// <summary>
        ///     Subscribes to events of the specified type with explicit priority and lifetime binding.
        /// </summary>
        /// <param name="action">The event handler callback.</param>
        /// <param name="priority">The dispatch priority. Higher values are invoked first.</param>
        /// <param name="lifetime">The lifetime boundary; the subscription is automatically cancelled when the lifetime ends.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <returns>A subscription handle.</returns>
        ISubscription Subscribe<TEvent>(Action<TEvent> action, int priority, ILifetime lifetime)
            where TEvent : IEvent;

        /// <summary>
        ///     Cancels the specified event subscription.
        /// </summary>
        /// <param name="subscription">The subscription handle to cancel.</param>
        void Unsubscribe(ISubscription subscription);

        /// <summary>
        ///     Publishes an event of the specified type, notifying all subscribers in priority order.
        /// </summary>
        /// <param name="payload">The event payload.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        void Publish<TEvent>(in TEvent payload)
            where TEvent : IEvent;

        /// <summary>
        ///     Gets the current total number of event subscriptions.
        /// </summary>
        /// <returns>The number of event subscriptions.</returns>
        int GetEventSubscriptionCount();
    }
}