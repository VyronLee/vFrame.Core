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
    /// <summary>
    /// Represents a terminal destroy contract. Once <see cref="Destroy"/> completes,
    /// the instance must remain in the destroyed state and is not expected to re-enter
    /// a usable lifecycle.
    /// </summary>
    public interface IDestroyable : IDisposable
    {
        bool Destroyed { get; }

        /// <summary>
        /// Executes one-shot teardown for the instance.
        /// </summary>
        void Destroy();
    }
}
