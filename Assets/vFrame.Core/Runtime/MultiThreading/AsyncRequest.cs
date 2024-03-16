using System;
using vFrame.Core.Base;

namespace vFrame.Core.MultiThreading
{
    public class AsyncRequest<TArg> : BaseObject<TArg>, IAsyncRequest, IDisposable
    {
        protected TArg Arg { get; private set; }
        protected bool IsDestroyed { get; private set; }

        protected override void OnCreate(TArg arg) {
            Arg = arg;
            IsDestroyed = false;
        }

        protected override void OnDestroy() {
            Arg = default;
            IsDestroyed = true;
        }

        public void Dispose() {
            if (IsDestroyed) {
                return;
            }
            Destroy();
        }

        public bool MoveNext() {
            return !IsDone;
        }

        public void Reset() {
        }

        public object Current => null;

        public bool IsDone { get; protected set; }
        public float Progress { get; protected set; }
    }

    public class AsyncRequest<TRet, TArg> : BaseObject<TArg>, IAsyncRequest<TRet>, IDisposable
    {
        public TRet Value { get; protected set; }
        protected TArg Arg { get; private set; }
        protected bool IsDestroyed { get; private set; }

        protected override void OnCreate(TArg arg) {
            Arg = arg;
            IsDestroyed = false;
        }

        protected override void OnDestroy() {
            Arg = default;
            Value = default;
            IsDestroyed = true;
        }

        public void Dispose() {
            if (IsDestroyed) {
                return;
            }
            Destroy();
        }

        public bool MoveNext() {
            return !IsDone;
        }

        public void Reset() {
            Value = default;
        }

        public object Current => Value;

        public bool IsDone { get; protected set; }
        public float Progress { get; protected set; }
    }
}