using vFrame.Core.Base;

namespace vFrame.Core.Dispatchers
{
    public interface ISubscription : IDestroyable
    {
        uint Handle { get; }
    }
}
