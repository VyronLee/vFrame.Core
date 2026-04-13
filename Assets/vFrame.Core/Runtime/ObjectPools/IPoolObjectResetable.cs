// ------------------------------------------------------------
//         File: IPoolObjectResetable.cs
//        Brief: Interface for resettable pool objects, invoked automatically on return
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-07-09 19:09:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    public interface IPoolObjectResetable
    {
        /// <summary>
        ///     Resets the object to a clean state so it can be safely reused from the pool.
        /// </summary>
        void Reset();
    }
}