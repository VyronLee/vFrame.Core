// ------------------------------------------------------------
//         File: ThreadedTask.cs
//        Brief: Threaded task implementations that execute work on the ThreadPool.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-02-15 20:05
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Threading;

namespace vFrame.Core
{
    public abstract class ThreadedTask<TArg> : Task<TArg>
    {
        private readonly object _lockObject = new object();
        private readonly WaitCallback _runTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadedTask{TArg}"/> class.
        /// </summary>
        protected ThreadedTask() {
            _runTask = state => RunTask();
        }

        /// <inheritdoc/>
        protected override void OnCreate(TArg arg) {
            base.OnCreate(arg);

            if (!ThreadPool.QueueUserWorkItem(_runTask)) {
                ThrowHelper.ThrowUndesiredException("Queue work item to thread pool failed.");
            }
        }

        /// <inheritdoc/>
        public override void RunTask() {
            if (Destroyed) {
                return;
            }

            try {
                OnHandleTask(Arg);
            }
            catch (Exception e) {
                ErrorHandler(e);
            }

            lock (_lockObject) {
                IsDone = true;
                Progress = 1f;
            }
        }

        /// <summary>
        /// Handles an exception that occurred during task execution.
        /// </summary>
        /// <param name="e">The exception to handle.</param>
        protected virtual void ErrorHandler(Exception e) {
            Logger.Error(e.ToString());
        }

        /// <summary>
        /// Executes the task logic with the specified argument.
        /// </summary>
        /// <param name="arg">The task argument.</param>
        protected abstract void OnHandleTask(TArg arg);
    }

    public abstract class ThreadedTask<TRet, TArg> : Task<TRet, TArg>
    {
        private readonly object _lockObject = new object();
        private readonly WaitCallback _runTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadedTask{TRet, TArg}"/> class.
        /// </summary>
        protected ThreadedTask() {
            _runTask = state => RunTask();
        }

        /// <inheritdoc/>
        protected override void OnCreate(TArg arg) {
            base.OnCreate(arg);

            if (!ThreadPool.QueueUserWorkItem(_runTask)) {
                ThrowHelper.ThrowUndesiredException("Queue work item to thread pool failed.");
            }
        }

        /// <inheritdoc/>
        public override void RunTask() {
            if (Destroyed) {
                return;
            }

            try {
                Value = OnHandleTask(Arg);
            }
            catch (Exception e) {
                ErrorHandler(e);
            }

            lock (_lockObject) {
                IsDone = true;
                Progress = 1f;
            }
        }

        /// <summary>
        /// Handles an exception that occurred during task execution.
        /// </summary>
        /// <param name="e">The exception to handle.</param>
        protected virtual void ErrorHandler(Exception e) {
            Logger.Error(e.ToString());
        }

        /// <summary>
        /// Executes the task logic and returns a result.
        /// </summary>
        /// <param name="arg">The task argument.</param>
        /// <returns>The task result.</returns>
        protected abstract TRet OnHandleTask(TArg arg);
    }
}
