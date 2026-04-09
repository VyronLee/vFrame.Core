// ------------------------------------------------------------
//         File: StringBuilderPool.cs
//        Brief: Preconfigured object pool for <see cref="System.Text.StringBuilder"/> instances
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-04-16 15:49:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Text;

namespace vFrame.Core
{
    public class StringBuilderPool : ObjectPool<StringBuilder, StringBuilderAllocator> { }
}
