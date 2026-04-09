// ------------------------------------------------------------
//         File: DictionaryAllocator.cs
//        Brief: Pool object allocator for generic Dictionary instances
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-07-09 19:34:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;

namespace vFrame.Core
{
    public class DictionaryAllocator<T1, T2> : IPoolObjectAllocator<Dictionary<T1, T2>>
    {
        public static int PresetLength = 64;

        /// <summary>
        /// Allocates a new <see cref="Dictionary{TKey,TValue}"/> with the configured preset capacity.
        /// </summary>
        /// <returns>A new dictionary instance.</returns>
        public Dictionary<T1, T2> Alloc() {
            return new Dictionary<T1, T2>(PresetLength);
        }

        /// <summary>
        /// Resets the given dictionary by clearing all entries.
        /// </summary>
        /// <param name="obj">The dictionary to reset.</param>
        public void Reset(Dictionary<T1, T2> obj) {
            obj.Clear();
        }
    }
}
