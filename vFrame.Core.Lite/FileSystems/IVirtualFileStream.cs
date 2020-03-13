using System;
using System.IO;

namespace vFrame.Core.FileSystems
{
    public interface IVirtualFileStream : IDisposable
    {
        long Position { get; set; }

        void Flush();

        long Seek(long offset, SeekOrigin origin);

        void SetLength(long value);

        int Read(byte[] buffer, int offset, int count);

        void Write(byte[] buffer, int offset, int count);
    }
}