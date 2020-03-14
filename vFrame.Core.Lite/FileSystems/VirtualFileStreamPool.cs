using System.IO;
using Microsoft.IO;
using vFrame.Core.Singletons;

namespace vFrame.Core.FileSystems
{
    internal class VirtualFileStreamPool : Singleton<VirtualFileStreamPool>
    {
        private const int BlockSize = 1024;
        private const int LargeBufferMultiple = 1024 * 1024;
        private const int MaxBufferSize = 16 * LargeBufferMultiple;

        private RecyclableMemoryStreamManager _streamManager;

        protected override void OnCreate() {
            _streamManager = new RecyclableMemoryStreamManager {
                GenerateCallStacks = true,
                AggressiveBufferReturn = true,
                MaximumFreeLargePoolBytes = MaxBufferSize * 4,
                MaximumFreeSmallPoolBytes = 100 * BlockSize
            };
        }

        public MemoryStream GetStream() {
            return _streamManager.GetStream();
        }
    }
}