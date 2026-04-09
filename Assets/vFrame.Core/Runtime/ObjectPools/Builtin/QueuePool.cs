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
    public class QueuePool<T> : ObjectPool<Queue<T>, QueueAllocator<T>> { }
}
