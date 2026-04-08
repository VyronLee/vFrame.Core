using System;
using vFrame.Core.Base;

namespace vFrame.Core.Dispatchers
{
    public interface IEventDispatcher
    {
        ISubscription Subscribe<TEvent>(Action<TEvent> action)
            where TEvent : IEvent;

        ISubscription Subscribe<TEvent>(Action<TEvent> action, BaseObject owner)
            where TEvent : IEvent;

        ISubscription Subscribe<TEvent>(Action<TEvent> action, ILifetime lifetime)
            where TEvent : IEvent;

        void Unsubscribe(ISubscription subscription);

        void Publish<TEvent>(in TEvent payload)
            where TEvent : IEvent;

        int GetEventSubscriptionCount();
    }
}
