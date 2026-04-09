// ------------------------------------------------------------
//         File: HashSetPool.cs
//        Brief: Object pool for generic HashSet instances
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-07-09 19:34:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;

namespace vFrame.Core
{
    public class HashSetPool<T> : ObjectPool<HashSet<T>, HashSetAllocator<T>> { }
}
