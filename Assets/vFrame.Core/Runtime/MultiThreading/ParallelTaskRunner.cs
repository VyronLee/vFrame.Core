using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using vFrame.Core.Base;
using vFrame.Core.Loggers;

namespace vFrame.Core.MultiThreading
{
    public class ParallelTaskRunner<T> : CreateAbility<ParallelTaskRunner<T>, int>
    {
        private const int MaxThreadCount = 32;
        private const int DefaultThreadCount = 4;
        private readonly WaitCallback _taskHandler;
        private int _threadCount;
        private bool _abortOnError;
        private SpinLock _finishedListLock;
        private Exception _lastError;
        private Action _onComplete;
        private Action<Exception> _onError;

        private Action<T> _onHandle;
        private int _taskFinishedCount;
        private int _taskTotalCount;
        private List<T> _taskWaiting;
        private CancellationTokenSource[] _tokenSources;
        private SpinLock _waitingListLock;

        private ParallelTaskRunner() {
            _abortOnError = true;
            _waitingListLock = new SpinLock();
            _taskHandler = HandleTask;
        }

        public static ParallelTaskRunner<T> Spawn() {
            return Create(DefaultThreadCount);
        }

        public static ParallelTaskRunner<T> Spawn(int threadCount) {
            return Create(threadCount);
        }

        protected override void OnCreate(int threadCount) {
            _threadCount = Math.Min(threadCount, MaxThreadCount);
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

        public async System.Threading.Tasks.Task<Exception> Wait() {
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
                if (!ConsumeTaskContext(out var task)) {
                    break;
                }
                try {
                    _onHandle?.Invoke(task);
                    HandleTaskComplete(task);
                }
                catch (Exception e) {
                    HandleError(e);
                }
                Thread.Sleep(1);
            }
        }

        private void HandleError(Exception e) {
            Interlocked.Exchange(ref _lastError, e);

            _onError?.Invoke(e);

            if (!_abortOnError) {
                return;
            }

            CancelAllTask();
        }

        private void HandleTaskComplete(T task) {
            if (Interlocked.Add(ref _taskFinishedCount, 1) < _taskTotalCount) {
                return;
            }

            if (null == _lastError) {
                _onComplete?.Invoke();
            }
        }

        private void CancelAllTask() {
            foreach (var source in _tokenSources) {
                source.Cancel();
            }
        }

        private bool ConsumeTaskContext(out T value) {
            var lockTaken = false;
            try {
                _waitingListLock.Enter(ref lockTaken);
                if (_taskWaiting.Count <= 0) {
                    value = default;
                    return false;
                }

                var context = _taskWaiting[0];
                _taskWaiting.RemoveAt(0);
                value = context;
                return true;
            }
            finally {
                if (lockTaken) {
                    _waitingListLock.Exit(false);
                }
            }
        }
    }
}