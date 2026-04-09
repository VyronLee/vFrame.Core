// ------------------------------------------------------------
//         File: CoroutineState.cs
//        Brief: Flags enum representing the execution state of
//                a coroutine (paused, running, stopped, finished).
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 22:00:00
//    Copyright: Copyright (c) 2019, VyronLee
// ============================================================

using System;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Bitwise flags describing the current state of a coroutine.
    /// </summary>
    [Flags]
    [Serializable]
    public enum CoroutineState
    {
        /// <summary>
        /// The coroutine is paused.
        /// </summary>
        Paused = 1,

        /// <summary>
        /// The coroutine is actively running.
        /// </summary>
        Running = 1 << 1,

        /// <summary>
        /// The coroutine has been stopped.
        /// </summary>
        Stopped = 1 << 2,

        /// <summary>
        /// The coroutine has completed execution.
        /// </summary>
        Finished = 1 << 3
    }
}
