// ------------------------------------------------------------
//         File: ParallelTaskRunner.cs
//        Brief: Parallel task runner that distributes work items across a thread pool.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-02-15 20:05
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace vFrame.Core
{
    public class ParallelTaskRunner<T> : BaseObject<int>
    {
        private const int MaxThreadCount = 32;
        private const int DefaultThreadCount = 4;
        private WaitCallback _taskHandler;
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

        /// <summary>
        /// Creates and initializes a new <see cref="ParallelTaskRunner{T}"/> with the default thread count.
        /// </summary>
        /// <returns>A new parallel task runner instance.</returns>
        public static ParallelTaskRunner<T> Spawn() {
            var ret = new ParallelTaskRunner<T>();
            ret.Create(DefaultThreadCount);
            return ret;
        }

        /// <summary>
        /// Creates and initializes a new <see cref="ParallelTaskRunner{T}"/> with the specified thread count.
        /// </summary>
        /// <param name="threadCount">The number of threads to use, capped at <see cref="MaxThreadCount"/>.</param>
        /// <returns>A new parallel task runner instance.</returns>
        public static ParallelTaskRunner<T> Spawn(int threadCount) {
            var ret = new ParallelTaskRunner<T>();
            ret.Create(threadCount);
            return ret;
        }

        /// <inheritdoc/>
        protected override void OnCreate(int threadCount) {
            _abortOnError = true;
            _waitingListLock = new SpinLock();
            _taskHandler = HandleTask;
            _threadCount = Math.Min(threadCount, MaxThreadCount);
        }

        /// <inheritdoc/>
        protected override void OnDestroy() {
            CancelAllTask();

            _tokenSources = null;
            _taskWaiting = null;
        }

        /// <summary>
        /// Submits a collection of items for parallel processing.
        /// </summary>
        /// <param name="contexts">The items to process.</param>
        /// <returns>This instance for fluent chaining.</returns>
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

        /// <summary>
        /// Sets the handler invoked for each work item.
        /// </summary>
        /// <param name="handler">The action to execute per item.</param>
        /// <returns>This instance for fluent chaining.</returns>
        public ParallelTaskRunner<T> OnHandle(Action<T> handler) {
            _onHandle = handler;
            return this;
        }

        /// <summary>
        /// Sets the handler invoked when all tasks complete successfully.
        /// </summary>
        /// <param name="handler">The completion callback.</param>
        /// <returns>This instance for fluent chaining.</returns>
        public ParallelTaskRunner<T> OnComplete(Action handler) {
            _onComplete = handler;
            return this;
        }

        /// <summary>
        /// Sets the handler invoked when a task encounters an error.
        /// </summary>
        /// <param name="handler">The error callback receiving the exception.</param>
        /// <returns>This instance for fluent chaining.</returns>
        public ParallelTaskRunner<T> OnError(Action<Exception> handler) {
            _onError = handler;
            return this;
        }

        /// <summary>
        /// Sets whether the runner should abort all remaining tasks on the first error.
        /// </summary>
        /// <param name="value"><c>true</c> to abort on error; otherwise, <c>false</c>.</param>
        /// <returns>This instance for fluent chaining.</returns>
        public ParallelTaskRunner<T> AbortOnError(bool value) {
            _abortOnError = value;
            return this;
        }

        /// <summary>
        /// Asynchronously waits for all tasks to complete or for an error to occur.
        /// </summary>
        /// <returns>The last exception encountered, or <c>null</c> if all tasks succeeded.</returns>
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

        /// <summary>
        /// Gets whether all tasks have finished processing.
        /// </summary>
        /// <returns><c>true</c> if all tasks are complete; otherwise, <c>false</c>.</returns>
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
