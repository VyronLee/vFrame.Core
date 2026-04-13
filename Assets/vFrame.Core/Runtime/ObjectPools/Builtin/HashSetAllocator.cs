// ------------------------------------------------------------
//         File: HashSetAllocator.cs
//        Brief: Pool object allocator for generic HashSet instances
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-07-09 19:34:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;

namespace vFrame.Core
{
    public class HashSetAllocator<T> : IPoolObjectAllocator<HashSet<T>>
    {
        /// <summary>
        ///     Allocates a new <see cref="HashSet{T}" /> instance.
        /// </summary>
        /// <returns>A new hash set instance.</returns>
        public HashSet<T> Alloc() {
            return new HashSet<T>();
        }

        /// <summary>
        ///     Resets the given hash set by clearing all elements.
        /// </summary>
        /// <param name="obj">The hash set to reset.</param>
        public void Reset(HashSet<T> obj) {
            obj.Clear();
        }
    }
}