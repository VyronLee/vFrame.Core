// ------------------------------------------------------------
//         File: CoroutineRunnerBehaviour.cs
//        Brief: MonoBehaviour that drives a single pooled coroutine
//               task through its lifecycle (start, pause, stop,
//               finish) and notifies the pool on completion.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 22:09:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections;
using UnityEngine;

namespace vFrame.Core.Unity
{
    internal class CoroutineRunnerBehaviour : MonoBehaviour
    {
        [SerializeField]
        private CoroutineTask _task;

        [SerializeField]
        private CoroutineState _state;

        [SerializeField]
        private int _runnerId;

        /// <summary>
        /// Callback invoked when the runner finishes its current task.
        /// </summary>
        public Action<CoroutineTask> OnFinished = null;

        /// <summary>
        /// Gets or sets the unique identifier of this runner within the pool.
        /// </summary>
        public int RunnerId {
            get => _runnerId;
            set => _runnerId = value;
        }

        /// <summary>
        /// Gets the handle of the currently assigned task.
        /// </summary>
        public int TaskHandle => _task.Handle;

        /// <summary>
        /// Resets the runner state to idle.
        /// </summary>
        private void Reset() {
            _state = 0;
        }

        /// <summary>
        /// Pauses the current coroutine execution.
        /// </summary>
        public void Pause() {
            _state |= CoroutineState.Paused;
        }

        /// <summary>
        /// Resumes the coroutine if it was previously paused.
        /// </summary>
        public void UnPause() {
            _state &= ~CoroutineState.Paused;
        }

        /// <summary>
        /// Starts executing the specified coroutine task.
        /// </summary>
        /// <param name="task">The coroutine task to execute.</param>
        /// <exception cref="CoroutinePoolInvalidStateException">
        /// Thrown when the runner is already executing a task.
        /// </exception>
        public void CoStart(CoroutineTask task) {
            if (IsRunning()) {
                throw new CoroutinePoolInvalidStateException("Coroutine is running, cannot start another task!");
            }

            Reset();

            _task = task;
            _state |= CoroutineState.Running;

            StartCoroutine(RunTask());
        }

        /// <summary>
        /// Stops the current coroutine and marks the runner as stopped.
        /// </summary>
        public void CoStop() {
            _state |= CoroutineState.Stopped;
            _state &= ~CoroutineState.Running;

            StopAllCoroutines();
        }

        /// <summary>
        /// Returns whether the runner is currently paused.
        /// </summary>
        /// <returns><c>true</c> if paused; otherwise, <c>false</c>.</returns>
        public bool IsPause() {
            return (_state & CoroutineState.Paused) > 0;
        }

        /// <summary>
        /// Returns whether the runner is currently executing a task.
        /// </summary>
        /// <returns><c>true</c> if running; otherwise, <c>false</c>.</returns>
        public bool IsRunning() {
            return (_state & CoroutineState.Running) > 0;
        }

        /// <summary>
        /// Returns whether the runner has finished its task.
        /// </summary>
        /// <returns><c>true</c> if finished; otherwise, <c>false</c>.</returns>
        public bool IsFinished() {
            return (_state & CoroutineState.Finished) > 0;
        }

        /// <summary>
        /// Returns whether the runner has been stopped.
        /// </summary>
        /// <returns><c>true</c> if stopped; otherwise, <c>false</c>.</returns>
        public bool IsStopped() {
            return (_state & CoroutineState.Stopped) > 0;
        }

        /// <summary>
        /// Coroutine loop that drives the assigned task, respecting pause
        /// and stop signals, and invokes <see cref="OnFinished"/> on completion.
        /// </summary>
        /// <returns>Enumerator for Unity coroutine scheduling.</returns>
        private IEnumerator RunTask() {
            var taskContext = _task;
            while (IsRunning()) {
                if (IsStopped()) {
                    break;
                }

                if (IsPause()) {
                    yield return null;
                    continue;
                }

                yield return taskContext.Task;

                _state |= CoroutineState.Finished;
                break;
            }

            _state &= ~CoroutineState.Running;
            _state |= CoroutineState.Stopped;
            _task = default;

            if (null != OnFinished) {
                OnFinished(taskContext);
            }
        }
    }
}