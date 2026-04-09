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
    public class
        SortedDictionaryPool<T1, T2> : ObjectPool<SortedDictionary<T1, T2>, SortedDictionaryAllocator<T1, T2>> { }
}
