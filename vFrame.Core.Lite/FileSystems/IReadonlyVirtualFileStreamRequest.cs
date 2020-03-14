using System;
using vFrame.Core.Base;

namespace vFrame.Core.FileSystems
{
    public interface IReadonlyVirtualFileStreamRequest : IAsync, IDisposable
    {
        IVirtualFileStream Stream { get; }
    }
}