//------------------------------------------------------------
//        File:  StackAllocator.cs
//       Brief:  Pool allocator for Stack instances.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-11-16 10:09
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Collections.Generic;

namespace vFrame.Core
{
    public class StackAllocator<T> : IPoolObjectAllocator<Stack<T>>
    {
        public static int PresetLength = 64;

        /// <summary>
        ///     Allocates a new Stack with the preset capacity.
        /// </summary>
        public Stack<T> Alloc() {
            return new Stack<T>(PresetLength);
        }

        /// <summary>
        ///     Resets the Stack by clearing all elements.
        /// </summary>
        public void Reset(Stack<T> obj) {
            obj.Clear();
        }
    }
}