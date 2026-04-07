using System;
using vFrame.Core.Base;

namespace vFrame.Core.EventDispatchers
{
    public interface IInteractionDispatcher
    {
        IInteractionSubscription Subscribe<TMessage>(Action<TMessage> action)
            where TMessage : class, IInteractionMessage;

        IInteractionSubscription Subscribe<TMessage>(Action<TMessage> action, BaseObject owner)
            where TMessage : class, IInteractionMessage;

        IInteractionSubscription Subscribe<TMessage>(Action<TMessage> action, ILifetime lifetime)
            where TMessage : class, IInteractionMessage;

        void Unsubscribe(ISubscription subscription);

        void Publish<TMessage>(TMessage message)
            where TMessage : class, IInteractionMessage;

        int GetInteractionSubscriptionCount();
    }
}
