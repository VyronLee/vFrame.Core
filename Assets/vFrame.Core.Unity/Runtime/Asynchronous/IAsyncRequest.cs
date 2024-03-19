using System;
using System.Collections;

namespace vFrame.Core.Unity.Asynchronous
{
    public enum AsyncState
    {
        NotStarted,
        Processing,
        Finished,
        Error,
    }

    public interface IAsyncRequest : IEnumerator
    {
        void Start();
        void Stop();
        void Update();
        AsyncState State { get; }
        bool IsDone { get; }
        bool IsError { get; }
        float Progress { get; }
        event Action OnFinish;
        event Action OnError;
    }
}