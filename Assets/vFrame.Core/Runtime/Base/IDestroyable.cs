// ------------------------------------------------------------
//         File: IDestroyable.cs
//        Brief: IDestroyable.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-18 16:0
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core.Base
{
    public interface IDestroyable : IDisposable
    {
        bool Destroyed { get; }
        void Destroy();
    }
}