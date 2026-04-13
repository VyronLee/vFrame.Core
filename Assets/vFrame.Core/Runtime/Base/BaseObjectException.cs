// ------------------------------------------------------------
//         File: BaseObjectException.cs
//        Brief: Exception types for BaseObject lifecycle violations
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2025-01-01 00:00:00
//    Copyright: Copyright (c) 2025, VyronLee
// ============================================================


namespace vFrame.Core
{
    /// <summary>
    ///     Thrown when a <see cref="BaseObject" /> is used after being destroyed.
    /// </summary>
    public class BaseObjectDestroyedException : vFrameException
    { }

    /// <summary>
    ///     Thrown when a <see cref="BaseObject" /> is used without being created first.
    /// </summary>
    public class BaseObjectNotCreatedException : vFrameException
    { }
}