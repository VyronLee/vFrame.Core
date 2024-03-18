using System.Collections;
using vFrame.Core.Unity.Coroutine;

namespace vFrame.Core.Unity.Asynchronous
{
    public interface IAsyncRequest : IEnumerator
    {
        void Setup(CoroutinePool pool);
    }
}