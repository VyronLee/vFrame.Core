using vFrame.Core.Base;

namespace vFrame.Core.MultiThreading
{
    public interface IAsyncRequest : IAsync
    {

    }

    public interface IAsyncRequest<out TRet> : IAsyncRequest
    {
        TRet Value { get; }
    }
}