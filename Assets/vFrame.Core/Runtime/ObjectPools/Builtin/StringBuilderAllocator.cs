//------------------------------------------------------------
//        File:  StringBuilderAllocator.cs
//       Brief:  Pool allocator for StringBuilder instances.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-07-09 19:27
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Text;

namespace vFrame.Core
{
    public class StringBuilderAllocator : IPoolObjectAllocator<StringBuilder>
    {
        public static int PresetLength = 1024;

        /// <summary>
        ///     Allocates a new StringBuilder with the preset capacity.
        /// </summary>
        public StringBuilder Alloc() {
            return new StringBuilder(PresetLength);
        }

        /// <summary>
        ///     Resets the StringBuilder by clearing its content.
        /// </summary>
        public void Reset(StringBuilder obj) {
            obj.Length = 0;
        }
    }
}