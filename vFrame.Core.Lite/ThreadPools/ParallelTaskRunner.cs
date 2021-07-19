using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using vFrame.Core.Base;

namespace vFrame.Core.ThreadPools
{
    public class ParallelTaskRunner<T> : BaseObject<int>
    {
        private ThreadPool _threadPool;
        private List<T> _taskContexts;
        private Action<T> _onHandle;
        private Action _onComplete;
        private Action<Exception> _onError;
        private Exception _lastError;
        private bool _abortOnError;
        private bool _destroyOnFinished;
        private SpinLock _spinLock;

        private ParallelTaskRunner() {
            _taskContexts = new List<T>();
            _destroyOnFinished = true;
            _abortOnError = true;
            _spinLock = new SpinLock();
        }

        public static ParallelTaskRunner<T> Spawn(int threadCount) {
            var runner = new ParallelTaskRunner<T>();
            runner.Create(threadCount);
            return runner;
        }

        protected override void OnCreate(int threadCount) {
            _threadPool = new ThreadPool();
            _threadPool.Create(threadCount);
        }

        protected override void OnDestroy() {
            _threadPool?.Destroy();
            _threadPool = null;

            _taskContexts.Clear();
            _taskContexts = null;
        }

        public ParallelTaskRunner<T> AddTask(T context) {
            var lockTaken = false;
            try {
                _spinLock.Enter(ref lockTaken);
                _taskContexts.Add(context);
            }
            finally {
                if (lockTaken) {
                    _spinLock.Exit(false);
                }
            }
            return this;
        }

        public ParallelTaskRunner<T> AddTask(IEnumerable<T> contexts) {
            var lockTaken = false;
            try {
                _spinLock.Enter(ref lockTaken);
                _taskContexts.AddRange(contexts);
            }
            finally {
                if (lockTaken) {
                    _spinLock.Exit(false);
                }
            }
            return this;
        }

        public ParallelTaskRunner<T> Start() {
            var lockTaken = false;
            try {
                _spinLock.Enter(ref lockTaken);
                foreach (var taskContext in _taskContexts) {
                    _threadPool.AddTask(HandleTask, taskContext, HandleError);
                }
            }
            finally {
                if (lockTaken) {
                    _spinLock.Exit(false);
                }
            }
            return this;
        }

        public ParallelTaskRunner<T> OnHandle(Action<T> handler) {
            _onHandle = handler;
            return this;
        }

        public ParallelTaskRunner<T> OnComplete(Action handler) {
            _onComplete = handler;
            return this;
        }

        public ParallelTaskRunner<T> OnError(Action<Exception> handler) {
            _onError = handler;
            return this;
        }

        public ParallelTaskRunner<T> AbortOnError(bool value) {
            _abortOnError = value;
            return this;
        }

        public ParallelTaskRunner<T> DestroyOnFinished(bool value) {
            _destroyOnFinished = value;
            return this;
        }

        public async Task<Exception> Wait() {
            while (null != _taskContexts && _taskContexts.Count > 0) {
                if (_abortOnError && null != _lastError) {
                    _threadPool?.Stop(true);
                    break;
                }
                await Task.Delay(1);
            }
            return _lastError;
        }

        private void HandleError(Exception e) {
            _lastError = e;
            _onError?.Invoke(e);
        }

        private void HandleTask(object state) {
            var context = (T) state;
            try {
                _onHandle?.Invoke(context);
            }
            finally {
                var lockTaken = false;
                try {
                    _spinLock.Enter(ref lockTaken);
                    _taskContexts.Remove(context);

                    if (_taskContexts.Count <= 0) {
                        _onComplete?.Invoke();

                        if (_destroyOnFinished) {
                            _threadPool?.Stop(true);
                        }
                    }
                }
                finally {
                    if (lockTaken) {
                        _spinLock.Exit(false);
                    }
                }
            }
        }
    }
}