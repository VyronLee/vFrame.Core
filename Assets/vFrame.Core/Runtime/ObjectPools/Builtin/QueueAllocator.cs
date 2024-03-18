//------------------------------------------------------------
//        File:  QueueAllocator.cs
//       Brief:  Queue allocator
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-11-16 10:11
//   Copyright:  Copyright (c) 2024, Author
//============================================================

using System.Collections.Generic;

namespace vFrame.Core.ObjectPools.Builtin
{
    public class QueueAllocator<T> : IPoolObjectAllocator<Queue<T>>
    {
        public static int PresetLength = 64;

        public Queue<T> Alloc() {
            return new Queue<T>(PresetLength);
        }

        public void Reset(Queue<T> obj) {
            obj.Clear();
        }
    }
}