using System;
using vFrame.Core.Base;

namespace vFrame.Core.MultiThreading
{
    public class AsyncRequest<TArg> : BaseObject<TArg>, IAsyncRequest
    {
        protected TArg Arg { get; private set; }

        protected override void OnCreate(TArg arg) {
            Arg = arg;
        }

        protected override void OnDestroy() {
            Arg = default;
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

    public class AsyncRequest<TRet, TArg> : BaseObject<TArg>, IAsyncRequest<TRet>
    {
        public TRet Value { get; protected set; }
        protected TArg Arg { get; private set; }

        protected override void OnCreate(TArg arg) {
            Arg = arg;
        }

        protected override void OnDestroy() {
            Arg = default;
            Value = default;
        }

        public bool MoveNext() {
            return !IsDone;
        }

        public void Reset() {
            Value = default;
        }

        public object Current => Value;

        public virtual bool IsDone { get; protected set; }
        public virtual float Progress { get; protected set; }
    }
}