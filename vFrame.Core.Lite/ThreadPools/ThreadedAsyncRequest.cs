using System;
using System.Collections.Generic;
using vFrame.Core.Loggers;

namespace vFrame.Core.ThreadPools
{
    public abstract class ThreadedAsyncRequest<TRet, TArg> : AsyncRequest<TRet, TArg>
    {
        protected static int ParallelCount = 4;

        private static readonly HashSet<TArg> Contexts = new HashSet<TArg>();
        private static readonly object LockObject = new object();

        private static ThreadPool _threadPool;

        private static ThreadPool GetOrSpawnThreadPool() {
            if (null == _threadPool) {
                _threadPool = new ThreadPool();
                _threadPool.Create(ParallelCount);
            }
            return _threadPool;
        }

        private static void ReleaseThreadPool() {
            _threadPool?.Destroy();
            _threadPool = null;
        }

        protected override void OnCreate(TArg arg) {
            base.OnCreate(arg);

            lock (LockObject) {
                Contexts.Add(arg);
                GetOrSpawnThreadPool().AddTask(RunTask, arg, ErrorHandler);
            }
        }

        protected override void OnDestroy() {
            if (null != Arg) {
                RemoveAndRelease(Arg);
            }
            base.OnDestroy();
        }

        private void RunTask(object state) {
            if (null == state) {
                throw new ArgumentNullException(nameof(state));
            }

            if (IsDestroyed) {
                return;
            }

            try {
                Value = OnThreadedHandle((TArg) state);
                IsDone = true;
                Progress = 1f;
            }
            finally {
                RemoveAndRelease((TArg) state);
            }
        }

        private static void RemoveAndRelease(TArg state) {
            lock (LockObject) {
                Contexts.Remove(state);
                if (Contexts.Count <= 0) {
                    ReleaseThreadPool();
                }
            }
        }

        protected virtual void ErrorHandler(Exception e) {
            Logger.Error(e.ToString());
        }

        protected abstract TRet OnThreadedHandle(TArg arg);
    }
}