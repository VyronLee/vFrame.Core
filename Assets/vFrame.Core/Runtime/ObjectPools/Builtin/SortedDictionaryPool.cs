//------------------------------------------------------------
//        File:  SortedDictionaryPool.cs
//       Brief:  Object pool for SortedDictionary instances.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-11-16 10:10
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Collections.Generic;

namespace vFrame.Core
{
    public class SortedDictionaryPool<TKey, TValue> : ObjectPool<SortedDictionary<TKey, TValue>>
    {
        private static readonly object _lock = new object();
        private static SortedDictionaryPool<TKey, TValue> _shared;

        public new static SortedDictionaryPool<TKey, TValue> Shared {
            get {
                if (_shared == null) {
                    lock (_lock) {
                        if (_shared == null) {
                            _shared = new SortedDictionaryPool<TKey, TValue>();
                        }
                    }
                }
                return _shared;
            }
        }

        public SortedDictionaryPool() : base(new AllocatorPooledObjectPolicy<SortedDictionary<TKey, TValue>, SortedDictionaryAllocator<TKey, TValue>>()) { }
    }
}