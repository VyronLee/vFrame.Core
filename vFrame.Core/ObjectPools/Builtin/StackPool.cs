//------------------------------------------------------------
//        File:  StackPool.cs
//       Brief:  Stack pool.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-11-16 10:10
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System.Collections.Generic;

namespace vFrame.Core.ObjectPools.Builtin
{
    public class StackPool<T> : ObjectPool<Stack<T>, StackAllocator<T>>
    {
    }
}
