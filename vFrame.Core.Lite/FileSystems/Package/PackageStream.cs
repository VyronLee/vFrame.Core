using System;
using System.IO;
using vFrame.Core.FileSystems.Exceptions;

namespace vFrame.Core.FileSystems.Package
{
    public class PackageStream : Stream
    {
        private readonly PackageBlock _block;
        private readonly FileAccess _mode;

        private MemoryStream _pkgStream;
        private bool _opened;
        private bool _closed;

        internal PackageStream(PackageBlock block) {
            _block = block;
            _mode = FileAccess.ReadWrite;
        }

        internal PackageStream(PackageBlock block, FileAccess mode) : this(block) {
            _mode = mode;
        }

        public bool Open(Stream pkgStream) {
            if (null == pkgStream) {
                throw new ArgumentException("Argument cannot be null: ", nameof(pkgStream));
            }
            return InternalOpen(pkgStream);
        }

        public override void Close() {
            _pkgStream.Close();
            _pkgStream.Dispose();

            _closed = true;

            base.Close();
        }

        public byte[] GetBuffer() {
            ValidateStreamState();
            return _pkgStream.GetBuffer();
        }

        public override void Flush() {
            ValidateStreamState();
            _pkgStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            ValidateStreamState();
            return _pkgStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            ValidateStreamState();
            return _pkgStream.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            ValidateStreamState();
            _pkgStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            ValidateStreamState();
            _pkgStream.Write(buffer, offset, count);
        }

        public override bool CanRead => _opened && !_closed && (_mode & FileAccess.Read) > 0;
        public override bool CanSeek => _opened && !_closed;
        public override bool CanWrite => _opened && !_closed && (_mode & FileAccess.Write) > 0;
        public override long Length => _block.OriginalSize;
        public override long Position { get; set; }

        //=======================================================//
        //                         Private                     ==//
        //=======================================================//

        private void ValidateStreamState() {
            if (!_opened) {
                throw new PackageStreamNotOpenedException();
            }
            if (_closed) {
                throw new PackageStreamClosedException();
            }
        }

        private bool InternalOpen(Stream stream) {
            if (stream.Length < _block.Offset + _block.CompressedSize) {

            }

        }

    }
}