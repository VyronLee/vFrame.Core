using vFrame.Core.Base;

namespace vFrame.Core.EventDispatchers
{
    public interface IInteractionSubscription : IDestroyable
    {
        uint Handle { get; }
    }
}
