using vFrame.Core.Base;

namespace vFrame.Core.FileSystems
{
    public interface IReadonlyVirtualFileStreamRequest : IAsync
    {
        IVirtualFileStream Stream { get; }
    }
}