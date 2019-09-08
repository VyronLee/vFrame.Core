//------------------------------------------------------------
//        File:  EventExecutorPool.cs
//       Brief:  Event executor pool.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-09-08 23:19
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================
using vFrame.Core.ObjectPools;

namespace vFrame.Core.Events
{
    internal class EventExecutorPool : ObjectPool<EventExecutor, EventExecutorAllocator>
    {
        
    }

    internal class EventExecutorAllocator : IPoolObjectAllocator<EventExecutor>
    {
        public EventExecutor Alloc()
        {
            return new EventExecutor();
        }

        public void Reset(EventExecutor obj)
        {
            obj.Activated = false;
            obj.Stopped = false;
            obj.Handle = 0;
            obj.Listener = null;
            obj.EventId = 0;
        }
    }
}