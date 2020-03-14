using System.IO;
using vFrame.Core.FileSystems.Adapters;
using vFrame.Core.Utils;

namespace vFrame.Core.FileSystems.Unity.Adapters
{
    internal class UnityFileStreamAdapter : FileStreamAdapter
    {
        private readonly Stream _fileStream;

        public UnityFileStreamAdapter(string path, FileMode mode, FileAccess access, FileShare share) {
#if UNITY_ANDROID
            if (!PathUtils.IsFileInPersistentDataPath(path))
            {
                var relativePath = PathUtils.AbsolutePathToRelativeStreamingAssetsPath(path);
                _fileStream = BetterStreamingAssets.OpenRead(relativePath);
            }
            else
#endif
            {
                _fileStream = new FileStream(path, mode, access, share);
            }
        }

        public override bool CanRead => _fileStream.CanRead;
        public override bool CanSeek => _fileStream.CanSeek;
        public override bool CanWrite => _fileStream.CanWrite;
        public override long Length => _fileStream.Length;

        public override long Position {
            get => _fileStream.Position;
            set => _fileStream.Position = value;
        }

        public override void Close() {
            _fileStream.Close();
            base.Close();
        }

        public override void Flush() {
            _fileStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return _fileStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return _fileStream.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            _fileStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            _fileStream.Write(buffer, offset, count);
        }
    }
}