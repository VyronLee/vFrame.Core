//------------------------------------------------------------
//        File:  QueueAllocator.cs
//       Brief:  Pool allocator for Queue instances.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-11-16 10:11
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Collections.Generic;

namespace vFrame.Core
{
    public class QueueAllocator<T> : IPoolObjectAllocator<Queue<T>>
    {
        public static int PresetLength = 64;

        /// <summary>
        ///     Allocates a new Queue with the preset capacity.
        /// </summary>
        public Queue<T> Alloc() {
            return new Queue<T>(PresetLength);
        }

        /// <summary>
        ///     Resets the Queue by clearing all elements.
        /// </summary>
        public void Reset(Queue<T> obj) {
            obj.Clear();
        }
    }
}