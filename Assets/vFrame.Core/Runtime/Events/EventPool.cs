//------------------------------------------------------------
//        File:  EventPool.cs
//       Brief:  Event pool
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-08 23:14
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using vFrame.Core.ObjectPools;

namespace vFrame.Core.Events
{
    internal class EventPool : ObjectPool<Event, EventAllocator>
    {
    }

    internal class EventAllocator : IPoolObjectAllocator<Event>
    {
        public Event Alloc() {
            return new Event();
        }

        public void Reset(Event obj) {
            obj.Context = null;
            obj.Target = null;
            obj.EventId = 0;
        }
    }
}