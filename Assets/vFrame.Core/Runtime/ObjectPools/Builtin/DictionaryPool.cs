// ------------------------------------------------------------
//         File: DictionaryPool.cs
//        Brief: Object pool for generic Dictionary instances
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-07-09 19:34:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;

namespace vFrame.Core
{
    public class DictionaryPool<TKey, TValue> : ObjectPool<Dictionary<TKey, TValue>>
    {
        private static readonly object _lock = new object();
        private static DictionaryPool<TKey, TValue> _shared;

        public DictionaryPool() : base(
            new AllocatorPooledObjectPolicy<Dictionary<TKey, TValue>, DictionaryAllocator<TKey, TValue>>()) { }

        public new static DictionaryPool<TKey, TValue> Shared {
            get {
                if (_shared == null) {
                    lock (_lock) {
                        if (_shared == null) {
                            _shared = new DictionaryPool<TKey, TValue>();
                        }
                    }
                }

                return _shared;
            }
        }
    }
}