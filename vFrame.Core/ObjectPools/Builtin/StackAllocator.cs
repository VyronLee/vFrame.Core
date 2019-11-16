//------------------------------------------------------------
//        File:  StackAllocator.cs
//       Brief:  Stack allocator
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-11-16 10:09
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================
using System.Collections.Generic;

namespace vFrame.Core.ObjectPools.Builtin
{
    public class StackAllocator<T> : IPoolObjectAllocator<Stack<T>>
    {
        public static int StackLength = 64;

        public Stack<T> Alloc() {
            return new Stack<T>(StackLength);
        }

        public void Reset(Stack<T> obj) {
            obj.Clear();
        }
    }
}
