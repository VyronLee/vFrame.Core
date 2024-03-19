using System;
using System.Collections;
using vFrame.Core.Base;
using vFrame.Core.Unity.Coroutine;

namespace vFrame.Core.Unity.Asynchronous
{
    public abstract class AsyncRequest : BaseObject, IAsyncRequest
    {
        private CoroutinePool _coroutinePool;
        private int _handle;
        private bool _setup;
        public bool IsFinished { get; protected set; }

        public void Setup(CoroutinePool pool) {
            if (_setup) {
                throw new AsyncRequestAlreadySetupException();
            }
            _setup = true;
            _coroutinePool = pool ?? throw new ArgumentNullException(nameof(pool));
            _handle = _coroutinePool.StartCoroutine(Process());
        }

        public bool MoveNext() {
            return !IsFinished;
        }

        public void Reset() { }

        public object Current { get; }

        protected override void OnDestroy() {
            if (_handle > 0) {
                _coroutinePool?.StopCoroutine(_handle);
            }
            _handle = 0;
            _coroutinePool = null;
            _setup = false;
            IsFinished = false;
        }

        private IEnumerator Process() {
            yield return OnProcess();
            IsFinished = true;
        }

        protected abstract IEnumerator OnProcess();
    }

    public abstract class AsyncRequest<TArg> : BaseObject<TArg>, IAsyncRequest
    {
        private CoroutinePool _coroutinePool;
        private int _handle;
        private bool _setup;
        protected TArg Arg;
        public bool IsFinished { get; protected set; }

        public void Setup(CoroutinePool pool) {
            if (_setup) {
                throw new AsyncRequestAlreadySetupException();
            }
            _setup = true;
            _coroutinePool = pool ?? throw new ArgumentNullException(nameof(pool));
            _handle = _coroutinePool.StartCoroutine(Process(Arg));
        }

        public bool MoveNext() {
            return !IsFinished;
        }

        public void Reset() { }

        public object Current { get; }

        protected override void OnDestroy() {
            Arg = default;
            IsFinished = false;

            if (_handle > 0) {
                _coroutinePool?.StopCoroutine(_handle);
            }
            _handle = 0;
            _coroutinePool = null;
            _setup = false;
        }

        protected override void OnCreate(TArg arg) {
            Arg = arg;
        }

        private IEnumerator Process(TArg arg) {
            yield return OnProcess(arg);
            IsFinished = true;
        }

        protected abstract IEnumerator OnProcess(TArg arg);
    }

    public abstract class AsyncRequest<TArg1, TArg2> : BaseObject<TArg1, TArg2>, IAsyncRequest
    {
        private CoroutinePool _coroutinePool;
        private int _handle;
        private bool _setup;

        protected TArg1 Arg1;
        protected TArg2 Arg2;
        public bool IsFinished { get; protected set; }

        public void Setup(CoroutinePool pool) {
            if (_setup) {
                throw new AsyncRequestAlreadySetupException();
            }
            _setup = true;
            _coroutinePool = pool ?? throw new ArgumentNullException(nameof(pool));
            _handle = _coroutinePool.StartCoroutine(Process(Arg1, Arg2));
        }

        public bool MoveNext() {
            return !IsFinished;
        }

        public void Reset() { }

        public object Current { get; }

        protected override void OnCreate(TArg1 arg1, TArg2 arg2) {
            Arg1 = arg1;
            Arg2 = arg2;
        }

        protected override void OnDestroy() {
            Arg1 = default;
            Arg2 = default;
            IsFinished = false;

            if (_handle > 0) {
                _coroutinePool?.StopCoroutine(_handle);
            }
            _handle = 0;
            _coroutinePool = null;
            _setup = false;
        }

        private IEnumerator Process(TArg1 arg1, TArg2 arg2) {
            yield return OnProcess(arg1, arg2);
            IsFinished = true;
        }

        protected abstract IEnumerator OnProcess(TArg1 arg1, TArg2 arg2);
    }

    public abstract class AsyncRequest<TArg1, TArg2, TArg3> : BaseObject<TArg1, TArg2, TArg3>, IAsyncRequest
    {
        private CoroutinePool _coroutinePool;
        private int _handle;
        private bool _setup;

        protected TArg1 Arg1;
        protected TArg2 Arg2;
        protected TArg3 Arg3;
        public bool IsFinished { get; protected set; }

        public void Setup(CoroutinePool pool) {
            if (_setup) {
                throw new AsyncRequestAlreadySetupException();
            }
            _setup = true;
            _coroutinePool = pool ?? throw new ArgumentNullException(nameof(pool));
            _handle = _coroutinePool.StartCoroutine(Process(Arg1, Arg2, Arg3));
        }

        public bool MoveNext() {
            return !IsFinished;
        }

        public void Reset() { }

        public object Current { get; }

        protected override void OnCreate(TArg1 arg1, TArg2 arg2, TArg3 arg3) {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        protected override void OnDestroy() {
            Arg1 = default;
            Arg2 = default;
            Arg3 = default;
            IsFinished = false;

            if (_handle > 0) {
                _coroutinePool?.StopCoroutine(_handle);
            }
            _handle = 0;
            _coroutinePool = null;
            _setup = false;
        }

        private IEnumerator Process(TArg1 arg1, TArg2 arg2, TArg3 arg3) {
            yield return OnProcess(arg1, arg2, arg3);
            IsFinished = true;
        }

        protected abstract IEnumerator OnProcess(TArg1 arg1, TArg2 arg2, TArg3 arg3);
    }
}