using vFrame.Core.Base;

namespace vFrame.Core.ThreadPools
{
    public interface IAsyncRequest<out TRet> : IAsync
    {
        TRet Value { get; }
    }
}