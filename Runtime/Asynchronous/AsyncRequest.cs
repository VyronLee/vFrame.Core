using System;
using vFrame.Core.Base;

namespace vFrame.Core.Unity.Asynchronous
{
    public abstract class AsyncRequest : BaseObject, IAsyncRequest
    {
        public AsyncState State { get; private set; }

        public void WithSharedCtrl() {
            AsyncRequestCtrl.Shared.AddRequest(this);
        }

        public void Start() {
            if (State != AsyncState.NotStarted) {
                return;
            }
            State = AsyncState.Processing;
            OnStart();
        }

        public void Stop() {
            if (State != AsyncState.Processing && State != AsyncState.Finished) {
                return;
            }
            State = AsyncState.NotStarted;
            OnStop();
        }

        public void Update() {
            if (State != AsyncState.Processing) {
                return;
            }
            OnUpdate();
        }

        public bool IsDone => State == AsyncState.Finished;
        public bool IsError => State == AsyncState.Error;

        public abstract float Progress { get; }
        public event Action OnFinish;
        public event Action OnError;

        public bool MoveNext() {
            return !IsDone && !IsError;
        }

        public void Reset() {
            Stop();
        }

        public object Current => null;

        protected override void OnCreate() {

        }

        protected override void OnDestroy() {
            Stop();
        }

        protected void Abort() {
            if (State == AsyncState.Error) {
                return;
            }
            State = AsyncState.Error;
            OnError?.Invoke();
        }

        protected void Finish() {
            if (State == AsyncState.Finished) {
                return;
            }
            State = AsyncState.Finished;
            OnFinish?.Invoke();
        }

        protected void ThrowIfNotFinished() {
            if (State != AsyncState.Finished) {
                throw new AsyncRequestNotFinishedException();
            }
        }

        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract void OnUpdate();
    }
}