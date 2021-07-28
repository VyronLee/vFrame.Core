using vFrame.Core.Base;

namespace vFrame.Core.MultiThreading
{
    public interface IAsyncRequest<out TRet> : IAsync
    {
        TRet Value { get; }
    }
}