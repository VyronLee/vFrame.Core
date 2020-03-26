using System;
using System.IO;
using System.Threading;
using vFrame.Core.FileSystems.Constants;
using vFrame.Core.FileSystems.Exceptions;
using vFrame.Core.Loggers;
using vFrame.Core.Profiles;

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

        public PackageReadonlyVirtualFileStreamRequest(Stream vpkStream, PackageBlockInfo blockInfo) {
            var context = new PackageStreamContext {
                Stream = vpkStream,
                BlockInfo = blockInfo,
            };
            VirtualFileSystemThreadPool.Instance().AddTask(OpenPackageStreamAsync, context);
        }

        private void OpenPackageStreamAsync(object state) {
            try {
                PerfProfile.Start(out var id);
                PerfProfile.Pin("PackageReadonlyVirtualFileStreamRequest:OpenPackageStreamAsync", id);

                var context = (PackageStreamContext) state;
                var vpkStream = context.Stream;
                using (vpkStream) {
                    var stream = new PackageVirtualFileStream(vpkStream, context.BlockInfo, FileAccess.Read);
                    if (!stream.Open())
                        throw new PackageStreamOpenFailedException();
                    Stream = stream;
                }

                PerfProfile.Unpin(id);

                lock (_lockObject) {
                    _finished = true;

                    if (!_disposed)
                        return;

                    Stream.Dispose();
                    Stream = null;
                }
            }
            catch (Exception e) {
                Logger.Error(PackageFileSystemConst.LogTag, "Error occurred while reading package: {0}", e);
            }
        }

        public override bool MoveNext() {
            lock (_lockObject) {
                return !_finished;
            }
        }
    }
}