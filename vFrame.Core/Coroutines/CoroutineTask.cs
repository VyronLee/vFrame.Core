using System.Collections;

namespace vFrame.Core.Coroutines
{
    public struct CoroutineTask
    {
        public int handle;
        public IEnumerator task;
        public int runnerId;
    }
}