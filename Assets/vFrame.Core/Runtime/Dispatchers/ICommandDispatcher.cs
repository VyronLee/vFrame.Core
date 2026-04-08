using System;
using vFrame.Core.Base;

namespace vFrame.Core.Dispatchers
{
    public interface ICommandDispatcher
    {
        ISubscription Handle<TCommand>(Action<TCommand> handler)
            where TCommand : ICommand;

        ISubscription Handle<TCommand>(Action<TCommand> handler, BaseObject owner)
            where TCommand : ICommand;

        ISubscription Handle<TCommand>(Action<TCommand> handler, ILifetime lifetime)
            where TCommand : ICommand;

        void Unhandle(ISubscription subscription);

        void Send<TCommand>(in TCommand command)
            where TCommand : ICommand;

        int GetCommandSubscriptionCount();
    }
}
