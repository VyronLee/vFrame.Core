using System;
using System.Collections;
using vFrame.Core.Base;
using vFrame.Core.Coroutine;

namespace vFrame.Core.Asynchronous
{
    public abstract class AsyncRequest : BaseObject, IAsyncRequest
    {
        public bool IsFinished { get; protected set; }
        private CoroutinePool _coroutinePool;
        private int _handle;
        private bool _setup;

        public void Setup(CoroutinePool pool) {
            if (_setup) {
                throw new AsyncRequestAlreadySetupException();
            }
            _setup = true;
            _coroutinePool = pool ?? throw new ArgumentNullException(nameof(pool));
            _handle = _coroutinePool.StartCoroutine(Process());
        }

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

        public bool MoveNext() {
            return !IsFinished;
        }

        public void Reset() {

        }

        public object Current { get; }
    }

    public abstract class AsyncRequest<TArg> : BaseObject<TArg>, IAsyncRequest
    {
        private CoroutinePool _coroutinePool;
        public bool IsFinished { get; protected set; }
        protected TArg Arg;
        private int _handle;
        private bool _setup;

        public void Setup(CoroutinePool pool) {
            if (_setup) {
                throw new AsyncRequestAlreadySetupException();
            }
            _setup = true;
            _coroutinePool = pool ?? throw new ArgumentNullException(nameof(pool));
            _handle = _coroutinePool.StartCoroutine(Process(Arg));
        }

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

        public bool MoveNext() {
            return !IsFinished;
        }

        public void Reset() {

        }

        public object Current { get; }
    }

    public abstract class AsyncRequest<TArg1, TArg2> : BaseObject<TArg1, TArg2>, IAsyncRequest
    {
        private CoroutinePool _coroutinePool;
        public bool IsFinished { get; protected set; }

        protected TArg1 Arg1;
        protected TArg2 Arg2;
        private int _handle;
        private bool _setup;

        public void Setup(CoroutinePool pool) {
            if (_setup) {
                throw new AsyncRequestAlreadySetupException();
            }
            _setup = true;
            _coroutinePool = pool ?? throw new ArgumentNullException(nameof(pool));
            _handle = _coroutinePool.StartCoroutine(Process(Arg1, Arg2));
        }

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

        public bool MoveNext() {
            return !IsFinished;
        }

        public void Reset() {

        }

        public object Current { get; }
    }

    public abstract class AsyncRequest<TArg1, TArg2, TArg3> : BaseObject<TArg1, TArg2, TArg3>, IAsyncRequest
    {
        private CoroutinePool _coroutinePool;
        public bool IsFinished { get; protected set; }

        protected TArg1 Arg1;
        protected TArg2 Arg2;
        protected TArg3 Arg3;
        private int _handle;
        private bool _setup;

        public void Setup(CoroutinePool pool) {
            if (_setup) {
                throw new AsyncRequestAlreadySetupException();
            }
            _setup = true;
            _coroutinePool = pool ?? throw new ArgumentNullException(nameof(pool));
            _handle = _coroutinePool.StartCoroutine(Process(Arg1, Arg2, Arg3));
        }

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

        public bool MoveNext() {
            return !IsFinished;
        }

        public void Reset() {

        }

        public object Current { get; }
    }
}