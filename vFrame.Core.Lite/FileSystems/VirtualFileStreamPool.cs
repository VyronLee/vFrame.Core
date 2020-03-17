using System.IO;
using Microsoft.IO;
using vFrame.Core.Loggers;
using vFrame.Core.Singletons;

namespace vFrame.Core.FileSystems
{
    internal class VirtualFileStreamPool : Singleton<VirtualFileStreamPool>
    {
        private RecyclableMemoryStreamManager _streamManager;

        protected override void OnCreate() {
            _streamManager = new RecyclableMemoryStreamManager();

            //_streamManager.UsageReport += (bytes, freeBytes, useBytes, poolFreeBytes) => {
            //    //long smallPoolInUseBytes, long smallPoolFreeBytes, long largePoolInUseBytes, long largePoolFreeBytes);
            //    Logger.Info("MemoryStream pool details, small pool in use: {0:n0}, small pool free: {1:n0}, large pool in use: {2:n0}, large pool free: {3:n0}",
            //        bytes, freeBytes, useBytes, poolFreeBytes);
            //};
        }

        public MemoryStream GetStream() {
            return _streamManager.GetStream();
        }

        public MemoryStream GetStream(string tag, int size) {
            return _streamManager.GetStream(tag, size);
        }
    }
}