// ------------------------------------------------------------
//         File: CoroutinePool.cs
//        Brief: Manages a pool of coroutine runners, scheduling
//                and dispatching coroutine tasks with capacity limits.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 22:09:00
//    Copyright: Copyright (c) 2019, VyronLee
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using vFrame.Core;
using Object = UnityEngine.Object;
using Debug = vFrame.Core.Unity.CoroutinePoolDebug;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Manages a pool of coroutine runners that execute and schedule
    /// coroutine tasks, supporting pause, resume, and queued dispatch.
    /// </summary>
    public class CoroutinePool
    {
        private static int _index;

        private static GameObject _parent;
        internal static readonly LogTag LogTag = new LogTag("CoroutinePool");

        private const int StoppedHandlesCleanupThreshold = 64;

        private readonly GameObject _holder;

        internal readonly int Capacity;
        internal readonly List<CoroutineRunnerBehaviour> RunnerList;
        internal readonly Queue<CoroutineTask> TasksWaiting;
        private int _taskHandle;

        private readonly Stack<CoroutineRunnerBehaviour> _idleRunners =
            new Stack<CoroutineRunnerBehaviour>();

        private readonly HashSet<int> _stoppedHandles = new HashSet<int>();
        private readonly Dictionary<int, Action> _completionCallbacks = new Dictionary<int, Action>();

        /// <summary>
        /// Initializes a new coroutine pool with the specified name and capacity.
        /// </summary>
        /// <param name="name">Display name for the pool holder GameObject.</param>
        /// <param name="capacity">Maximum number of concurrent coroutine runners.</param>
        public CoroutinePool(string name = null, int capacity = int.MaxValue) {
            Capacity = capacity;
            RunnerList = new List<CoroutineRunnerBehaviour>();

            TasksWaiting = new Queue<CoroutineTask>(16);

            _holder = new GameObject($"Pool_{++_index}({name ?? "Unnamed"})");
            _holder.AddComponent<CoroutinePoolBehaviour>().Pool = this;
            _holder.transform.SetParent(PoolParent.transform);
        }

        /// <summary>
        /// Gets the parent GameObject under which all pool holders are organized.
        /// </summary>
        private static GameObject PoolParent {
            get {
                if (_parent) {
                    return _parent;
                }
                return _parent = new GameObject("CoroutinePools").DontDestroyEx();
            }
        }

        /// <summary>
        /// Destroys all runners, clears waiting tasks, and removes the pool holder.
        /// </summary>
        public void Destroy() {
            foreach (var runner in RunnerList) {
                if (!runner) {
                    continue;
                }
                runner.CoStop();
                runner.gameObject.DestroyEx();
            }
            RunnerList.Clear();
            TasksWaiting.Clear();
            _idleRunners.Clear();
            _stoppedHandles.Clear();
            _completionCallbacks.Clear();

            if (_holder) {
                UnityEngine.Object.Destroy(_holder);
            }
        }

        /// <summary>
        /// Starts a coroutine and returns a handle for later control.
        /// </summary>
        /// <param name="task">The enumerator representing the coroutine.</param>
        /// <returns>A unique handle identifying the started coroutine.</returns>
        public int StartCoroutine(IEnumerator task) {
            return StartCoroutine(task, null);
        }

        /// <summary>
        /// Starts a coroutine with a completion callback and returns a handle for later control.
        /// </summary>
        /// <param name="task">The enumerator representing the coroutine.</param>
        /// <param name="onComplete">Callback invoked when the coroutine finishes naturally.</param>
        /// <returns>A unique handle identifying the started coroutine.</returns>
        public int StartCoroutine(IEnumerator task, Action onComplete) {
            var handle = GenerateTaskHandle();

            Debug.Log("CoroutinePool:StartCoroutine - handle: " + handle);

            if (null != onComplete) {
                _completionCallbacks[handle] = onComplete;
            }

            var context = new CoroutineTask { Handle = handle, Task = task };
#if DEBUG_COROUTINE_POOL
            context.Stack = new StackTrace(1, true).ToString();
#endif

            var runner = FindIdleRunner();
            if (null != runner) {
                Debug.Log("CoroutinePool:StartCoroutine - pool not full, start running task ..");
                RunTask(context, runner);
            }
            else {
                Debug.Log("CoroutinePool:StartCoroutine - pool is full, add to waiting list ..");
                TasksWaiting.Enqueue(context);
            }

            return handle;
        }

        /// <summary>
        /// Stops the coroutine identified by the given handle,
        /// removing it from both the waiting list and the running list.
        /// </summary>
        /// <param name="handle">The handle of the coroutine to stop.</param>
        public void StopCoroutine(int handle) {
            Debug.Log("CoroutinePool:StopCoroutine - Stopping coroutine: " + handle);

            _stoppedHandles.Add(handle);

            // Remove from waiting list (rebuild queue without the matched item)
            var newQueue = new Queue<CoroutineTask>();
            while (TasksWaiting.Count > 0) {
                var task = TasksWaiting.Dequeue();
                if (task.Handle != handle) {
                    newQueue.Enqueue(task);
                }
                else {
                    Debug.Log("CoroutinePool:StopCoroutine - Stopping coroutine, remove from waiting list: " + handle);
                    _completionCallbacks.Remove(handle);
                }
            }
            // Re-enqueue remaining items
            while (newQueue.Count > 0) {
                TasksWaiting.Enqueue(newQueue.Dequeue());
            }

            // Remove from running list
            foreach (var runner in RunnerList) {
                if (!runner.IsRunning()) {
                    continue;
                }
                if (runner.TaskHandle != handle) {
                    continue;
                }
                runner.CoStop();
                _idleRunners.Push(runner);
                _completionCallbacks.Remove(handle);
                Debug.Log("CoroutinePool:StopCoroutine - Stopping coroutine, remove from running list: " + handle);
                break;
            }
        }

        /// <summary>
        /// Pauses the coroutine identified by the given handle.
        /// </summary>
        /// <param name="handle">The handle of the coroutine to pause.</param>
        public void PauseCoroutine(int handle) {
            var found = false;
            foreach (var runner in RunnerList) {
                if (runner.TaskHandle != handle) {
                    continue;
                }
                runner.Pause();
                found = true;
                break;
            }
            if (!found) {
                Debug.Warning("CoroutinePool:PauseCoroutine - Handle not found: " + handle);
            }
        }

        /// <summary>
        /// Resumes the coroutine identified by the given handle.
        /// </summary>
        /// <param name="handle">The handle of the coroutine to unpause.</param>
        public void UnPauseCoroutine(int handle) {
            var found = false;
            foreach (var runner in RunnerList) {
                if (runner.TaskHandle != handle) {
                    continue;
                }
                runner.UnPause();
                found = true;
                break;
            }
            if (!found) {
                Debug.Warning("CoroutinePool:UnPauseCoroutine - Handle not found: " + handle);
            }
        }

        /// <summary>
        /// Gets the current state of the coroutine identified by the given handle.
        /// </summary>
        /// <param name="handle">The handle of the coroutine to query.</param>
        /// <returns>The coroutine state, or CoroutineState.Stopped if not found.</returns>
        public CoroutineState GetCoroutineState(int handle) {
            foreach (var runner in RunnerList) {
                if (runner.TaskHandle == handle) {
                    return runner.State;
                }
            }
            return CoroutineState.Stopped;
        }

        /// <summary>
        /// Generates a unique task handle by incrementing an internal counter.
        /// </summary>
        /// <returns>The next available task handle.</returns>
        private int GenerateTaskHandle() {
            return ++_taskHandle;
        }

        /// <summary>
        /// Assigns the task to the specified runner and starts execution.
        /// </summary>
        /// <param name="context">The coroutine task context.</param>
        /// <param name="runner">The target runner.</param>
        private void RunTask(CoroutineTask context, CoroutineRunnerBehaviour runner) {
            context.RunnerId = runner.RunnerId;
            Debug.Log("CoroutinePool:RunTask - Run task, runnerId: " + context.RunnerId);
            runner.CoStart(context);
        }

        /// <summary>
        /// Returns an existing runner or spawns new runners up to the specified index.
        /// </summary>
        /// <param name="runnerId">The index of the runner to retrieve or create.</param>
        /// <returns>The runner at the specified index.</returns>
        private CoroutineRunnerBehaviour GetOrSpawnRunner(int runnerId) {
            if (runnerId < 0 || runnerId >= Capacity) {
                ThrowHelper.ThrowIndexOutOfRangeException(0, Capacity - 1, runnerId);
            }
            if (runnerId < RunnerList.Count) {
                Debug.Log("CoroutinePool:GetOrSpawnRunner - runner exist: " + runnerId);
                return RunnerList[runnerId];
            }

            var idx = RunnerList.Count;
            while (idx <= runnerId) {
                var runner = new GameObject("Coroutine_" + idx).AddComponent<CoroutineRunnerBehaviour>();
                runner.transform.SetParent(_holder.transform);
                runner.RunnerId = idx;
                runner.CoStop();
                runner.OnFinished = OnRunnerFinished;
                RunnerList.Add(runner);
                ++idx;
            }
            Debug.Log("CoroutinePool:GetOrSpawnRunner - spawning new runner: " + runnerId);
            return RunnerList[runnerId];
        }

        /// <summary>
        /// Pops an idle runner from the stack, or spawns a new one if capacity allows.
        /// Returns null if all runners are busy and capacity is reached.
        /// </summary>
        /// <returns>An idle runner, or null if none available.</returns>
        private CoroutineRunnerBehaviour FindIdleRunner() {
            while (_idleRunners.Count > 0) {
                var runner = _idleRunners.Pop();
                if (runner.IsStopped()) {
                    return runner;
                }
            }
            if (RunnerList.Count < Capacity) {
                return GetOrSpawnRunner(RunnerList.Count);
            }
            return null;
        }

        /// <summary>
        /// Callback invoked by a runner when it finishes its current task.
        /// Handles completion callbacks and pushes the runner back to the idle stack.
        /// </summary>
        /// <param name="taskContext">The completed coroutine task.</param>
        private void OnRunnerFinished(CoroutineTask taskContext) {
            Debug.Log("CoroutinePool:OnRunnerFinished - task finished: " + taskContext.Handle);

            var runner = RunnerList[taskContext.RunnerId];

            // Handle completion callback (only for naturally finished coroutines)
            if ((runner.State & CoroutineState.Finished) != 0) {
                if (_completionCallbacks.TryGetValue(taskContext.Handle, out var callback)) {
                    _completionCallbacks.Remove(taskContext.Handle);
                    callback?.Invoke();
                }
            }

            // Check if this handle was requested to be stopped
            if (_stoppedHandles.Remove(taskContext.Handle)) {
                _completionCallbacks.Remove(taskContext.Handle);
            }

            _idleRunners.Push(runner);
        }

        /// <summary>
        /// Dequeues the next waiting task and runs it on an idle runner.
        /// </summary>
        /// <returns>True if a task was dispatched; false otherwise.</returns>
        private bool TryPopupAndRunNext() {
            if (TasksWaiting.Count <= 0) {
                return false;
            }

            // Drain stopped tasks from the front of the queue
            while (TasksWaiting.Count > 0) {
                var peekHandle = TasksWaiting.Peek().Handle;
                if (!_stoppedHandles.Contains(peekHandle)) {
                    break;
                }
                var task = TasksWaiting.Dequeue();
                _stoppedHandles.Remove(task.Handle);
                _completionCallbacks.Remove(task.Handle);
            }

            if (TasksWaiting.Count <= 0) {
                return false;
            }

            var runner = FindIdleRunner();
            if (null == runner) {
                return false;
            }

            var context = TasksWaiting.Dequeue();

            // Final check: was this task stopped between drain and dequeue?
            if (_stoppedHandles.Remove(context.Handle)) {
                _completionCallbacks.Remove(context.Handle);
                return true; // Consumed a stopped task, try next
            }

            Debug.Log("CoroutinePool:PopupAndRunNext - popup new task: " + context.Handle);
            RunTask(context, runner);
            return true;
        }

        /// <summary>
        /// Called each frame to dispatch waiting tasks to idle runners.
        /// </summary>
        internal void OnUpdate() {
            while (TryPopupAndRunNext()) {
                // nothing to do
            }

            // Periodically clean up stopped handles to prevent unbounded growth
            if (_stoppedHandles.Count > StoppedHandlesCleanupThreshold) {
                _stoppedHandles.Clear();
            }
        }
    }
}
