using System;
using System.Threading;
using vFrame.Core.Loggers;

namespace vFrame.Core.MultiThreading
{
    public abstract class ThreadedAsyncRequest<TArg> : AsyncRequest<TArg>
    {
        private readonly object _lockObject = new object();

        protected override void OnCreate(TArg arg) {
            base.OnCreate(arg);

            if (!ThreadPool.QueueUserWorkItem(RunTask, Arg)) {
                throw new Exception("Queue work item to thread pool failed.");
            }
        }

        private void RunTask(object state) {
            if (Destroyed) {
                return;
            }

            try {
                OnThreadedHandle((TArg)state);
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

        protected abstract void OnThreadedHandle(TArg arg);
    }

    public abstract class ThreadedAsyncRequest<TRet, TArg> : AsyncRequest<TRet, TArg>
    {
        private readonly object _lockObject = new object();

        protected override void OnCreate(TArg arg) {
            base.OnCreate(arg);

            if (!ThreadPool.QueueUserWorkItem(RunTask, Arg)) {
                throw new Exception("Queue work item to thread pool failed.");
            }
        }

        private void RunTask(object state) {
            if (Destroyed) {
                return;
            }

            try {
                Value = OnThreadedHandle((TArg)state);
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

        protected abstract TRet OnThreadedHandle(TArg arg);
    }
}