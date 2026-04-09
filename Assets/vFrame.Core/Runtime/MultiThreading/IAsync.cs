// ------------------------------------------------------------
//         File: IAsync.cs
//        Brief: Interface for asynchronous operations with progress tracking.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-02-15 20:05
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections;

namespace vFrame.Core
{
    public interface IAsync : IEnumerator
    {
        /// <summary>
        /// Gets whether the asynchronous operation has completed.
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// Gets the current progress of the asynchronous operation, ranging from 0 to 1.
        /// </summary>
        float Progress { get; }
    }
}
