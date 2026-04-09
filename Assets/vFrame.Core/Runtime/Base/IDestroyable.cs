// ------------------------------------------------------------
//         File: IDestroyable.cs
//        Brief: Terminal destroy contract for one-shot teardown
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 16:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    /// <summary>
    /// Represents a terminal destroy contract. Once <see cref="Destroy"/> completes,
    /// the instance must remain in the destroyed state and is not expected to re-enter
    /// a usable lifecycle.
    /// </summary>
    public interface IDestroyable : IDisposable
    {
        /// <summary>
        /// Gets whether the instance has been destroyed.
        /// </summary>
        bool Destroyed { get; }

        /// <summary>
        /// Executes one-shot teardown for the instance.
        /// </summary>
        void Destroy();
    }
}
