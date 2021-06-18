using System.IO;

namespace vFrame.Core.FileSystems.Package
{
    internal class PackageVirtualFileSystemStream : Stream
    {
        private readonly Stream _origin;
        private readonly bool _leaveOpen;

        public PackageVirtualFileSystemStream(Stream other, bool leaveOpen) {
            _origin = other;
            _leaveOpen = leaveOpen;
        }

        protected override void Dispose(bool disposing) {
            if (!_leaveOpen) {
                _origin?.Close();
                _origin?.Dispose();
            }
            base.Dispose(disposing);
        }

        public override void Flush() {
            _origin?.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return _origin.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return _origin.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            _origin.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            _origin.Write(buffer, offset, count);
        }

        public override bool CanRead => _origin?.CanRead ?? false;
        public override bool CanSeek => _origin?.CanSeek ?? false;
        public override bool CanWrite => _origin?.CanWrite ?? false;
        public override long Length => _origin?.Length ?? 0;

        public override long Position {
            get => _origin.Position;
            set => _origin.Position = value;
        }
    }
}