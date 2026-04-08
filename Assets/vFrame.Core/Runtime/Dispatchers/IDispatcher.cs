namespace vFrame.Core.Dispatchers
{
    public interface IDispatcher : IEventDispatcher, ICommandDispatcher, IRequestDispatcher, IDecisionDispatcher
    {
        void RemoveAllSubscriptions();

        int GetTotalSubscriptionCount();
    }
}
