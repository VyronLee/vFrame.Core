// ------------------------------------------------------------
//         File: Task.cs
//        Brief: Abstract base task classes with argument and optional return value support.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-02-15 20:05
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    public abstract class Task<TArg> : BaseObject<TArg>, ITask
    {
        protected TArg Arg { get; private set; }

        /// <inheritdoc/>
        public bool MoveNext() {
            return !IsDone;
        }

        /// <inheritdoc/>
        public void Reset() { }

        /// <inheritdoc/>
        public object Current => null;

        /// <inheritdoc/>
        public bool IsDone { get; protected set; }

        /// <inheritdoc/>
        public float Progress { get; protected set; }

        /// <inheritdoc/>
        protected override void OnCreate(TArg arg) {
            Arg = arg;
        }

        /// <inheritdoc/>
        protected override void OnDestroy() {
            Arg = default;
        }

        /// <inheritdoc/>
        public abstract void RunTask();
    }

    public abstract class Task<TRet, TArg> : BaseObject<TArg>, ITask<TRet>
    {
        protected TArg Arg { get; private set; }

        /// <inheritdoc/>
        public TRet Value { get; protected set; }

        /// <inheritdoc/>
        public bool MoveNext() {
            return !IsDone;
        }

        /// <inheritdoc/>
        public void Reset() {
            Value = default;
        }

        /// <inheritdoc/>
        public object Current => Value;

        /// <inheritdoc/>
        public virtual bool IsDone { get; protected set; }

        /// <inheritdoc/>
        public virtual float Progress { get; protected set; }

        /// <inheritdoc/>
        protected override void OnCreate(TArg arg) {
            Arg = arg;
        }

        /// <inheritdoc/>
        protected override void OnDestroy() {
            Arg = default;
            Value = default;
        }

        /// <inheritdoc/>
        public abstract void RunTask();
    }
}
