using vFrame.Core.Exceptions;

namespace vFrame.Core.Unity.SpawnPools
{
    public class SpawnPoolException : vFrameException
    {
        public SpawnPoolException() {}
        public SpawnPoolException(string message) : base(message) {}
    }
}