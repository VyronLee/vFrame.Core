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
    public class HashSetPool<T> : ObjectPool<HashSet<T>>
    {
        private static readonly object _lock = new object();
        private static HashSetPool<T> _shared;

        public new static HashSetPool<T> Shared {
            get {
                if (_shared == null) {
                    lock (_lock) {
                        if (_shared == null) {
                            _shared = new HashSetPool<T>();
                        }
                    }
                }
                return _shared;
            }
        }

        public HashSetPool() : base(new AllocatorPooledObjectPolicy<HashSet<T>, HashSetAllocator<T>>()) { }
    }
}