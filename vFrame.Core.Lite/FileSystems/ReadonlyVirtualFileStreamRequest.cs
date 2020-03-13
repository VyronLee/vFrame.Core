namespace vFrame.Core.FileSystems
{
    internal abstract class ReadonlyVirtualFileStreamRequest : IReadonlyVirtualFileStreamRequest
    {
        public abstract bool MoveNext();

        public void Reset() {
            Stream = null;
        }

        public object Current => Stream;
        public bool IsDone => !MoveNext();
        public float Progress { get; }

        public IVirtualFileStream Stream { get; protected set; }
    }
}