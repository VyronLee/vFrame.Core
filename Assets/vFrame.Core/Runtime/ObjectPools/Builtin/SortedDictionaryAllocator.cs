//------------------------------------------------------------
//        File:  SortedDictionaryAllocator.cs
//       Brief:  Pool allocator for SortedDictionary instances.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-11-16 10:09
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Collections.Generic;

namespace vFrame.Core
{
    public class SortedDictionaryAllocator<T1, T2> : IPoolObjectAllocator<SortedDictionary<T1, T2>>
    {
        /// <summary>
        ///     Allocates a new SortedDictionary.
        /// </summary>
        public SortedDictionary<T1, T2> Alloc() {
            return new SortedDictionary<T1, T2>();
        }

        /// <summary>
        ///     Resets the SortedDictionary by clearing all entries.
        /// </summary>
        public void Reset(SortedDictionary<T1, T2> obj) {
            obj.Clear();
        }
    }
}