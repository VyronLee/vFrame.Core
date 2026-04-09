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
    public class ListPool<T> : ObjectPool<List<T>, ListAllocator<T>> { }
}
