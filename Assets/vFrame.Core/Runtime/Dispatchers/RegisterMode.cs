// ------------------------------------------------------------
//         File: RegisterMode.cs
//        Brief: Enum controlling behavior when registering duplicate handlers
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-12
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

namespace vFrame.Core
{
    /// <summary>
    ///     Controls the behavior when registering a handler for a message type that
    ///     already has a registered handler (applicable to Command and Request dispatchers
    ///     which allow only one handler per type).
    /// </summary>
    public enum RegisterMode
    {
        /// <summary>
        ///     Silently replaces the existing handler with the new one.
        ///     The old subscription is destroyed and returned to the pool.
        /// </summary>
        Replace,

        /// <summary>
        ///     Throws an <see cref="System.InvalidOperationException" /> if a handler
        ///     is already registered for the same message type.
        /// </summary>
        Throw,

        /// <summary>
        ///     Silently ignores the new registration if a handler already exists.
        ///     The existing handler remains active.
        /// </summary>
        Ignore
    }
}