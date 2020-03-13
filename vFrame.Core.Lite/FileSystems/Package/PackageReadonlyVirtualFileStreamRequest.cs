using System.IO;
using System.Threading;
using vFrame.Core.FileSystems.Exceptions;

namespace vFrame.Core.FileSystems.Package
{
    internal class PackageReadonlyVirtualFileStreamRequest : ReadonlyVirtualFileStreamRequest
    {
        private class PackageStreamContext
        {
            public Stream Stream;
            public PackageBlockInfo BlockInfo;
        }

        private bool _finished;

        private readonly object _lockObject = new object();

        public PackageReadonlyVirtualFileStreamRequest(Stream vpkStream, PackageBlockInfo blockInfo) {
            var context = new PackageStreamContext {
                Stream = vpkStream,
                BlockInfo = blockInfo,
            };
            ThreadPool.QueueUserWorkItem(OpenPackageStreamAsync, context);
        }

        private void OpenPackageStreamAsync(object state) {
            var context = (PackageStreamContext) state;
            var vpkStream = context.Stream;
            using (vpkStream) {
                var stream = new PackageVirtualFileStream(vpkStream, context.BlockInfo, FileAccess.Read);
                if (!stream.Open())
                    throw new PackageStreamOpenFailedException();
                Stream = stream;
            }

            lock (_lockObject) {
                _finished = true;
            }
        }

        public override bool MoveNext() {
            lock (_lockObject) {
                return !_finished;
            }
        }
    }
}