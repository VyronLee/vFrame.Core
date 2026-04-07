using vFrame.Core.Base;

namespace vFrame.Core.EventDispatchers
{
    public interface ISubscription : IDestroyable
    {
        uint Handle { get; }
    }
}
