// ------------------------------------------------------------
//         File: ILifetime.cs
//        Brief: Grouped-cleanup primitive for ownership boundaries
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
    /// Lightweight grouped-cleanup primitive for retained ownership boundaries.
    /// A lifetime can own child lifetimes, destroyables, and cleanup actions, but it is
    /// not intended to grow into a container or orchestration framework.
    /// </summary>
    public interface ILifetime : IDestroyable
    {
        /// <summary>
        /// Creates a child lifetime that ends when this lifetime ends.
        /// </summary>
        ILifetime CreateChild();

        /// <summary>
        /// Binds a destroyable to this lifetime so it is terminated when the lifetime ends.
        /// </summary>
        void Add(IDestroyable destroyable);

        /// <summary>
        /// Binds a cleanup action to this lifetime so it executes when the lifetime ends.
        /// </summary>
        void Add(Action action);
    }
}
