using System.Collections;
using vFrame.Core.Coroutine;

namespace vFrame.Core.Asynchronous
{
    public interface IAsyncRequest : IEnumerator
    {
        void Setup(CoroutinePool pool);
    }
}