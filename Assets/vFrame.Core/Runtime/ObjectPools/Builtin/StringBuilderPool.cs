//------------------------------------------------------------
//        File:  FixedStringBuilderPool.cs
//       Brief:  FixedStringBuilderPool
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-04-16 15:49
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Text;

namespace vFrame.Core.ObjectPools.Builtin
{
    public class StringBuilderPool : ObjectPool<StringBuilder, StringBuilderAllocator> { }
}