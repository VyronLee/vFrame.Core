//------------------------------------------------------------
//        File:  QueuePool.cs
//       Brief:  Object pool for Queue instances.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-11-16 10:10
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Collections.Generic;

namespace vFrame.Core
{
    public class QueuePool<T> : ObjectPool<Queue<T>>
    {
        private static readonly object _lock = new object();
        private static QueuePool<T> _shared;

        public QueuePool() : base(new AllocatorPooledObjectPolicy<Queue<T>, QueueAllocator<T>>()) { }

        public new static QueuePool<T> Shared {
            get {
                if (_shared == null) {
                    lock (_lock) {
                        if (_shared == null) {
                            _shared = new QueuePool<T>();
                        }
                    }
                }

                return _shared;
            }
        }
    }
}