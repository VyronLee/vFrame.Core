// ------------------------------------------------------------
//         File: StringBuilderPool.cs
//        Brief: Preconfigured object pool for <see cref="System.Text.StringBuilder"/> instances
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-04-16 15:49:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Text;

namespace vFrame.Core
{
    public class StringBuilderPool : ObjectPool<StringBuilder>
    {
        private static readonly object _lock = new object();
        private static StringBuilderPool _shared;

        public StringBuilderPool() : base(new AllocatorPooledObjectPolicy<StringBuilder, StringBuilderAllocator>()) { }

        public new static StringBuilderPool Shared {
            get {
                if (_shared == null) {
                    lock (_lock) {
                        if (_shared == null) {
                            _shared = new StringBuilderPool();
                        }
                    }
                }

                return _shared;
            }
        }
    }
}