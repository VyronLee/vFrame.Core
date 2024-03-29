using vFrame.Core.Base;

namespace vFrame.Core.MultiThreading
{
    public abstract class Task<TArg> : BaseObject<TArg>, ITask
    {
        protected TArg Arg { get; private set; }

        public bool MoveNext() {
            return !IsDone;
        }

        public void Reset() { }

        public object Current => null;

        public bool IsDone { get; protected set; }
        public float Progress { get; protected set; }

        protected override void OnCreate(TArg arg) {
            Arg = arg;
        }

        protected override void OnDestroy() {
            Arg = default;
        }

        public abstract void RunTask();
    }

    public abstract class Task<TRet, TArg> : BaseObject<TArg>, ITask<TRet>
    {
        protected TArg Arg { get; private set; }
        public TRet Value { get; protected set; }

        public bool MoveNext() {
            return !IsDone;
        }

        public void Reset() {
            Value = default;
        }

        public object Current => Value;

        public virtual bool IsDone { get; protected set; }
        public virtual float Progress { get; protected set; }

        protected override void OnCreate(TArg arg) {
            Arg = arg;
        }

        protected override void OnDestroy() {
            Arg = default;
            Value = default;
        }

        public abstract void RunTask();
    }
}