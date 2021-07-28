using System;
using System.Threading;
using vFrame.Core.Loggers;

namespace vFrame.Core.MultiThreading
{
    public abstract class ThreadedAsyncRequest<TRet, TArg> : AsyncRequest<TRet, TArg>
    {
        protected override void OnCreate(TArg arg) {
            base.OnCreate(arg);
            ThreadPool.QueueUserWorkItem(RunTask, Arg);
        }

        private void RunTask(object state) {
            if (IsDestroyed) {
                return;
            }

            Value = OnThreadedHandle((TArg) state);
            IsDone = true;
            Progress = 1f;
        }

        protected virtual void ErrorHandler(Exception e) {
            Logger.Error(e.ToString());
        }

        protected abstract TRet OnThreadedHandle(TArg arg);
    }
}