// ------------------------------------------------------------
//         File: ITask.cs
//        Brief: Task interface definitions extending IAsync with run capability.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-02-15 20:05
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    /// <summary>
    ///     Represents a runnable asynchronous task.
    /// </summary>
    public interface ITask : IAsync
    { }

    /// <summary>
    ///     Represents a runnable asynchronous task that produces a result.
    /// </summary>
    /// <typeparam name="TRet">The type of the task result.</typeparam>
    public interface ITask<out TRet> : ITask
    {
        /// <summary>
        ///     Gets the result of the task.
        /// </summary>
        TRet Value { get; }
    }
}