//------------------------------------------------------------
//        File:  ThreadPool.cs
//       Brief:  ThreadPool
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Modified:  2019-05-22 15:52
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System;
using System.Collections.Generic;
using System.Threading;
using vFrame.Core.Base;
using vFrame.Core.Loggers;

namespace vFrame.Core.ThreadPools
{
    public sealed class ThreadPool : BaseObject<int>
    {
        private static readonly LogTag ThreadPoolLogTag = new LogTag("ThreadPool");

        private class TaskContext
        {
            public WaitCallback callback;
            public object param;
            public ExceptionHandler handler;
        }

        public delegate void ExceptionHandler(Exception e);

        private class ThreadContext
        {
            public Thread thread;
            public bool stopped;
        }

        private readonly object _lockObject = new object();

        private List<ThreadContext> _threads;

        private Queue<TaskContext> _waitingTask;

        protected override void OnCreate(int threadCount) {
            lock (_lockObject) {
                _waitingTask = new Queue<TaskContext>();
            }

            _threads = new List<ThreadContext>(threadCount);
            for (var i = 0; i < threadCount; i++) {
                var thread = new Thread(ThreadProc);
                var context = new ThreadContext {thread = thread, stopped = false};
                _threads.Add(context);

                thread.Start(context);
            }
        }

        protected override void OnDestroy() {
            Stop();

            _threads = null;
            _waitingTask = null;
        }

        public void Stop(bool force = false) {
            foreach (var threadContext in _threads) {
                threadContext.stopped = true;
            }

            if (force) {
                foreach (var threadContext in _threads) {
                    threadContext.thread.Interrupt();
                }
            }
        }

        public void AddTask(WaitCallback callBack, object param = null, ExceptionHandler handler = null) {
            var context = new TaskContext {
                callback = callBack,
                param = param,
                handler = handler,
            };

            lock (_lockObject) {
                _waitingTask.Enqueue(context);
            }
        }

        public void ResumeStoppedThreads() {
            foreach (var threadContext in _threads)
                if (threadContext.stopped)
                    threadContext.thread.Start(threadContext);
        }

        private void ThreadProc(object stateInfo) {
            try {
                var threadContext = stateInfo as ThreadContext;
                if (threadContext == null)
                    throw new ArgumentNullException(nameof(stateInfo));

                while (!threadContext.stopped) {
                    Thread.Sleep(1);

                    TaskContext task = null;
                    lock (_lockObject) {
                        if (null != _waitingTask && _waitingTask.Count > 0)
                            task = _waitingTask.Dequeue();
                    }

                    if (task == null)
                        continue;

                    try {
                        task.callback(task.param);
                    }
                    catch (Exception e) {
                        if (task.handler != null)
                            task.handler(e);
                        else
                            Logger.Error(ThreadPoolLogTag, "Error occured in thread, exception: {0}", e);
                    }
                }
            }
            catch (ThreadInterruptedException) {
                // Thread force stopped.
            }
        }
    }
}