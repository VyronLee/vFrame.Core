//------------------------------------------------------------
//        File:  ListAllocator.cs
//       Brief:  ListAllocator
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-07-09 19:34
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System.Collections.Generic;

namespace vFrame.Core.ObjectPools.Builtin
{
    public class ListAllocator<T> : IPoolObjectAllocator<List<T>>
    {
        public static int PresetLength = 64;

        public List<T> Alloc() {
            return new List<T>(PresetLength);
        }

        public void Reset(List<T> obj) {
            obj.Clear();
        }
    }
}