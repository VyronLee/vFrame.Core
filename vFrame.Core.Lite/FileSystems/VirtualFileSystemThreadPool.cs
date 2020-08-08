using System;
using System.Threading;
using vFrame.Core.Singletons;
using ThreadPool = vFrame.Core.ThreadPools.ThreadPool;

namespace vFrame.Core.FileSystems
{
    internal class VirtualFileSystemThreadPool : Singleton<VirtualFileSystemThreadPool>
    {
        //private ThreadPool _threadPools;
        //private const int MaxThreadCount = 8;

        protected override void OnCreate() {
            //_threadPools = new ThreadPool();
            //_threadPools.Create(MaxThreadCount);
        }

        public void AddTask(WaitCallback callBack, object param = null, ThreadPool.ExceptionHandler handler = null) {
            //_threadPools?.AddTask(callBack, param, handler);

            System.Threading.ThreadPool.QueueUserWorkItem(state => {
                try {
                    callBack(param);
                }
                catch (Exception e) {
                    handler?.Invoke(e);
                }
            });
        }
    }
}