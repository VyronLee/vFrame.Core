// ------------------------------------------------------------
//         File: ListAllocator.cs
//        Brief: Pool object allocator for generic List instances
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-07-09 19:34:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;

namespace vFrame.Core
{
    public class ListAllocator<T> : IPoolObjectAllocator<List<T>>
    {
        public int PresetLength = 64;

        /// <summary>
        ///     Allocates a new <see cref="List{T}" /> with the configured preset capacity.
        /// </summary>
        /// <returns>A new list instance.</returns>
        public List<T> Alloc() {
            return new List<T>(PresetLength);
        }

        /// <summary>
        ///     Resets the given list by clearing all elements.
        /// </summary>
        /// <param name="obj">The list to reset.</param>
        public void Reset(List<T> obj) {
            obj.Clear();
        }
    }
}