//------------------------------------------------------------
//        File:  ListPool.cs
//       Brief:  Object pool for List instances.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-07-09 19:44
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Collections.Generic;

namespace vFrame.Core
{
    public class ListPool<T> : ObjectPool<List<T>>
    {
        private static readonly object _lock = new object();
        private static ListPool<T> _shared;

        public ListPool() : base(new AllocatorPooledObjectPolicy<List<T>, ListAllocator<T>>()) { }

        public new static ListPool<T> Shared {
            get {
                if (_shared == null) {
                    lock (_lock) {
                        if (_shared == null) {
                            _shared = new ListPool<T>();
                        }
                    }
                }

                return _shared;
            }
        }
    }
}