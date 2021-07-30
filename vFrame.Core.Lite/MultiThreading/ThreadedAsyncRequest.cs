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
            ThreadPool.QueueUserWorkItem(RunTask, Arg);
        }

        private void RunTask(object state) {
            if (IsDestroyed) {
                return;
            }

            try {
                OnThreadedHandle((TArg) state);
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
            ThreadPool.QueueUserWorkItem(RunTask, Arg);
        }

        private void RunTask(object state) {
            if (IsDestroyed) {
                return;
            }

            try {
                Value = OnThreadedHandle((TArg) state);
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