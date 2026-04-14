//------------------------------------------------------------
//        File:  StackPool.cs
//       Brief:  Object pool for Stack instances.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-11-16 10:10
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Collections.Generic;

namespace vFrame.Core
{
    public class StackPool<T> : ObjectPool<Stack<T>>
    {
        private static readonly object _lock = new object();
        private static StackPool<T> _shared;

        public new static StackPool<T> Shared {
            get {
                if (_shared == null) {
                    lock (_lock) {
                        if (_shared == null) {
                            _shared = new StackPool<T>();
                        }
                    }
                }
                return _shared;
            }
        }

        public StackPool() : base(new AllocatorPooledObjectPolicy<Stack<T>, StackAllocator<T>>()) { }
    }
}