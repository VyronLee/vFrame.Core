using System;
using System.Threading;
using vFrame.Core.Exceptions;
using vFrame.Core.Loggers;

namespace vFrame.Core.MultiThreading
{
    public abstract class ThreadedTask<TArg> : Task<TArg>
    {
        private readonly object _lockObject = new object();
        private readonly WaitCallback _runTask;

        protected ThreadedTask() {
            _runTask = state => RunTask();
        }

        protected override void OnCreate(TArg arg) {
            base.OnCreate(arg);

            if (!ThreadPool.QueueUserWorkItem(_runTask)) {
                ThrowHelper.ThrowUndesiredException("Queue work item to thread pool failed.");
            }
        }

        public override void RunTask() {
            if (Destroyed) {
                return;
            }

            try {
                OnHandleTask(Arg);
            }
            catch (Exception e) {
                ErrorHandler(e);
                return;
            }

            lock (_lockObject) {
                IsDone = true;
                Progress = 1f;
            }
        }

        protected virtual void ErrorHandler(Exception e) {
            Logger.Error(e.ToString());
        }

        protected abstract void OnHandleTask(TArg arg);
    }

    public abstract class ThreadedTask<TRet, TArg> : Task<TRet, TArg>
    {
        private readonly object _lockObject = new object();
        private readonly WaitCallback _runTask;

        protected ThreadedTask() {
            _runTask = state => RunTask();
        }

        protected override void OnCreate(TArg arg) {
            base.OnCreate(arg);

            if (!ThreadPool.QueueUserWorkItem(_runTask)) {
                ThrowHelper.ThrowUndesiredException("Queue work item to thread pool failed.");
            }
        }

        public override void RunTask() {
            if (Destroyed) {
                return;
            }

            try {
                Value = OnHandleTask(Arg);
            }
            catch (Exception e) {
                ErrorHandler(e);
                return;
            }

            lock (_lockObject) {
                IsDone = true;
                Progress = 1f;
            }
        }

        protected virtual void ErrorHandler(Exception e) {
            Logger.Error(e.ToString());
        }

        protected abstract TRet OnHandleTask(TArg arg);
    }
}