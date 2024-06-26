﻿//------------------------------------------------------------
//        File:  QueuePool.cs
//       Brief:  Queue pool.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-11-16 10:10
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Collections.Generic;

namespace vFrame.Core.ObjectPools.Builtin
{
    public class QueuePool<T> : ObjectPool<Queue<T>, QueueAllocator<T>> { }
}