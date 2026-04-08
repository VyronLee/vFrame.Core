using System;
using vFrame.Core.Base;

namespace vFrame.Core.Dispatchers
{
    public interface IDecisionDispatcher
    {
        ISubscription Listen<TDecision>(Func<TDecision, bool> handler)
            where TDecision : IDecision;

        ISubscription Listen<TDecision>(Func<TDecision, bool> handler, BaseObject owner)
            where TDecision : IDecision;

        ISubscription Listen<TDecision>(Func<TDecision, bool> handler, ILifetime lifetime)
            where TDecision : IDecision;

        void Unlisten(ISubscription subscription);

        bool Decide<TDecision>(in TDecision decision)
            where TDecision : IDecision;

        int GetDecisionSubscriptionCount();
    }
}
