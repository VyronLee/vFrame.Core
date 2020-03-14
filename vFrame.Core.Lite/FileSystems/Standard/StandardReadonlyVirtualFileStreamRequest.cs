using System.IO;
using System.Threading;

namespace vFrame.Core.FileSystems.Standard
{
    internal class StandardReadonlyVirtualFileStreamRequest : ReadonlyVirtualFileStreamRequest
    {
        private bool _finished;

        public StandardReadonlyVirtualFileStreamRequest(Stream stream) {
            ThreadPool.QueueUserWorkItem(ReadFileStream, stream);
        }

        private void ReadFileStream(object state) {
            var stream  = (Stream) state;
            using (stream) {
                var memoryStream = VirtualFileStreamPool.Instance().GetStream();
                stream.CopyTo(memoryStream);
                Stream = new StandardVirtualFileStream(memoryStream);
                Stream.SetLength(stream.Length);
            }

            lock (_lockObject) {
                _finished = true;

                if (!_disposed)
                    return;
                Stream.Dispose();
                Stream = null;
            }
        }

        public override bool MoveNext() {
            lock (_lockObject) {
                return !_finished;
            }
        }
    }
}