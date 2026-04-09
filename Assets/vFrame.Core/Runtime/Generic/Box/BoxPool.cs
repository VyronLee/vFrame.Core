// ------------------------------------------------------------
//         File: BoxPool.cs
//        Brief: Object pool for <see cref="Box{T}"/> instances.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-20 16:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using vFrame.Core;

namespace vFrame.Core
{
    public class BoxPool<T> : ObjectPool<Box<T>, BoxAllocator<T>> { }
}
