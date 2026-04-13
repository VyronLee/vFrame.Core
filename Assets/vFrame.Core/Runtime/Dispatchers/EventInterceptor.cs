// ------------------------------------------------------------
//         File: EventInterceptor.cs
//        Brief: Pre-publish and post-publish interceptor hooks for the event dispatcher
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-12
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    /// <summary>
    ///     An interceptor that can observe, modify, or short-circuit event publishing.
    ///     Interceptors are invoked in registration order before and after each Publish call.
    /// </summary>
    public interface IEventInterceptor
    {
        /// <summary>
        ///     Called before an event is published. Return <c>false</c> to short-circuit
        ///     and prevent the event from being dispatched to subscribers.
        /// </summary>
        /// <param name="eventType">The runtime type of the event being published.</param>
        /// <param name="eventData">The event payload (boxed). May be modified by reference for struct events.</param>
        /// <returns><c>true</c> to allow publishing; <c>false</c> to cancel.</returns>
        bool OnBeforePublish(Type eventType, ref IEvent eventData);

        /// <summary>
        ///     Called after an event has been published to all subscribers.
        /// </summary>
        /// <param name="eventType">The runtime type of the event that was published.</param>
        /// <param name="eventData">The event payload.</param>
        /// <param name="subscriberCount">The number of subscribers that received the event.</param>
        void OnAfterPublish(Type eventType, IEvent eventData, int subscriberCount);
    }
}