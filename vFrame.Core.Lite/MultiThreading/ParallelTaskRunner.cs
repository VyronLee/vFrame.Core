using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using vFrame.Core.Base;
using vFrame.Core.Loggers;

namespace vFrame.Core.MultiThreading
{
    public class ParallelTaskRunner<T> : BaseObject
    {
        private const int MaxThreadCount = 32;
        private const int DefaultThreadCount = 4;

        private Action<T> _onHandle;
        private Action _onComplete;
        private Action<Exception> _onError;
        private Exception _lastError;
        private bool _abortOnError;
        private SpinLock _waitingListLock;
        private SpinLock _finishedListLock;
        private CancellationTokenSource[] _tokenSources;
        private List<T> _taskWaiting;
        private int _taskTotalCount;
        private int _taskFinishedCount;
        private readonly int _threadCount;
        private readonly WaitCallback _taskHandler;

        private ParallelTaskRunner() {
            _abortOnError = true;
            _waitingListLock = new SpinLock();
            _taskHandler = HandleTask;
        }

        private ParallelTaskRunner(int threadCount) : this() {
            _threadCount = Math.Min(threadCount, MaxThreadCount);
        }

        public static ParallelTaskRunner<T> Spawn() {
            var runner = new ParallelTaskRunner<T>(DefaultThreadCount);
            runner.Create();
            return runner;
        }

        public static ParallelTaskRunner<T> Spawn(int threadCount) {
            var runner = new ParallelTaskRunner<T>(threadCount);
            runner.Create();
            return runner;
        }

        protected override void OnCreate() {
        }

        protected override void OnDestroy() {
            CancelAllTask();

            _tokenSources = null;
            _taskWaiting = null;
        }

        public ParallelTaskRunner<T> Run(IEnumerable<T> contexts) {
            _taskWaiting = contexts.ToList();
            _taskTotalCount = _taskWaiting.Count;
            _taskFinishedCount = 0;

            _tokenSources = new CancellationTokenSource[_threadCount];

            for (var i = 0; i < _threadCount; i++) {
                var source = _tokenSources[i] = new CancellationTokenSource();
                if (!ThreadPool.QueueUserWorkItem(_taskHandler, source)) {
                    Logger.Error("Queue work item to thread pool failed: {0}", i);
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

        public async Task<Exception> Wait() {
            var delay = Task.Delay(1);
            while (!IsComplete()) {
                if (_abortOnError && null != _lastError) {
                    break;
                }
                await delay;
            }
            return _lastError;
        }

        public bool IsComplete() {
            return _taskFinishedCount >= _taskTotalCount;
        }

        private void HandleTask(object state) {
            var source = state as CancellationTokenSource;
            while (!source.IsCancellationRequested) {
                var task = ConsumeTaskContext();
                if (null == task) {
                    break;
                }
                try {
                    _onHandle?.Invoke(task);
                    HandleTaskComplete(task);
                }
                catch (Exception e) {
                    HandleError(e);
                }
            }
        }

        private void HandleError(Exception e) {
            Interlocked.Exchange(ref _lastError, e);

            _onError?.Invoke(e);

            if (!_abortOnError)
                return;

            CancelAllTask();
        }

        private void HandleTaskComplete(T task) {
            if (Interlocked.Add(ref _taskFinishedCount, 1) < _taskTotalCount)
                return;

            if (null == _lastError) {
                _onComplete?.Invoke();
            }
        }

        private void CancelAllTask() {
            foreach (var source in _tokenSources) {
                source.Cancel();
            }
        }

        private T ConsumeTaskContext() {
            var lockTaken = false;
            try {
                _waitingListLock.Enter(ref lockTaken);
                if (_taskWaiting.Count <= 0) {
                    return default;
                }

                var context = _taskWaiting[0];
                _taskWaiting.RemoveAt(0);
                return context;
            }
            finally {
                if (lockTaken) {
                    _waitingListLock.Exit(false);
                }
            }
        }
    }
}