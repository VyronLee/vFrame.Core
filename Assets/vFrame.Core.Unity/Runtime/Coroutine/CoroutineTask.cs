// ------------------------------------------------------------
//         File: CoroutineTask.cs
//        Brief: Data structure representing a coroutine task
//                with its handle, runner assignment, and debug info.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 22:11:00
//    Copyright: Copyright (c) 2019, VyronLee
// ============================================================

using System;
using System.Collections;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Holds the context for a single coroutine task, including its
    /// handle, assigned runner, optional stack trace, and enumerator.
    /// </summary>
    [Serializable]
    internal struct CoroutineTask
    {
        /// <summary>
        /// Unique handle identifying this coroutine task.
        /// </summary>
        public int Handle;

        /// <summary>
        /// Index of the runner executing this task.
        /// </summary>
        public int RunnerId;

        /// <summary>
        /// Captured stack trace for debug builds.
        /// </summary>
        public string Stack;

        /// <summary>
        /// The enumerator representing the coroutine body.
        /// </summary>
        public IEnumerator Task;
    }
}
