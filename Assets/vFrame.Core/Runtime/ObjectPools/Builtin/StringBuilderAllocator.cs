//------------------------------------------------------------
//        File:  StringBuilderAllocator.cs
//       Brief:  StringBuilderAllocator
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-07-09 19:27
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Text;

namespace vFrame.Core.ObjectPools.Builtin
{
    public class StringBuilderAllocator : IPoolObjectAllocator<StringBuilder>
    {
        public static int PresetLength = 1024;

        public StringBuilder Alloc() {
            return new StringBuilder(PresetLength);
        }

        public void Reset(StringBuilder obj) {
            obj.Length = 0;
        }
    }
}