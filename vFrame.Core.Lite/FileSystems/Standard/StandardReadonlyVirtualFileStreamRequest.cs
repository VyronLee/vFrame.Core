using System;
using System.IO;
using System.Threading;
using vFrame.Core.Extensions;
using vFrame.Core.FileSystems.Constants;
using vFrame.Core.Loggers;

namespace vFrame.Core.FileSystems.Standard
{
    internal class StandardReadonlyVirtualFileStreamRequest : ReadonlyVirtualFileStreamRequest
    {
        private bool _finished;

        public StandardReadonlyVirtualFileStreamRequest(Stream stream) {
            ThreadPool.QueueUserWorkItem(ReadFileStream, stream);
        }

        private void ReadFileStream(object state) {
            try {
                var stream  = (Stream) state;
                using (stream) {
                    var memoryStream = VirtualFileStreamPool.Instance().GetStream();
                    stream.BufferedCopyTo(memoryStream, (int)stream.Length);
                    Stream = new StandardVirtualFileStream(memoryStream);
                }

                lock (_lockObject) {
                    _finished = true;

                    if (!_disposed)
                        return;
                    Stream.Dispose();
                    Stream = null;
                }
            }
            catch (Exception e) {
                Logger.Error(PackageFileSystemConst.LogTag, "Error occurred while reading file: {0}", e);
            }
        }

        public override bool MoveNext() {
            lock (_lockObject) {
                return !_finished;
            }
        }
    }
}