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

using System.Collections;
using System.Collections.Generic;
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

        private readonly GameObject _holder;

        internal readonly int Capacity;
        internal readonly List<CoroutineRunnerBehaviour> RunnerList;
        internal readonly List<CoroutineTask> TasksWaiting;
        private int _taskHandle;

        /// <summary>
        /// Initializes a new coroutine pool with the specified name and capacity.
        /// </summary>
        /// <param name="name">Display name for the pool holder GameObject.</param>
        /// <param name="capacity">Maximum number of concurrent coroutine runners.</param>
        public CoroutinePool(string name = null, int capacity = int.MaxValue) {
            Capacity = capacity;
            RunnerList = new List<CoroutineRunnerBehaviour>();

            TasksWaiting = new List<CoroutineTask>(16);

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
            var handle = GenerateTaskHandle();

            Debug.Log("CoroutinePool:StartCoroutine - handle: " + handle);

            var context = new CoroutineTask { Handle = handle, Task = task };
#if DEBUG_COROUTINE_POOL
            context.Stack = StackTraceUtility.ExtractStackTrace();
#endif

            var runnerId = FindIdleRunner();
            if (runnerId >= 0) {
                Debug.Log("CoroutinePool:StartCoroutine - pool not full, start running task ..");
                RunTask(context, runnerId);
            }
            else {
                Debug.Log("CoroutinePool:StartCoroutine - pool is full, add to waiting list ..");
                TasksWaiting.Add(context);
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
            // Remove from waiting list
            for (var i = 0; i < TasksWaiting.Count; ++i) {
                if (TasksWaiting[i].Handle != handle) {
                    continue;
                }
                TasksWaiting.RemoveAt(i);
                Debug.Log("CoroutinePool:StopCoroutine - Stopping coroutine, remove from waiting list: " + handle);
                break;
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
                Debug.Log("CoroutinePool:StopCoroutine - Stopping coroutine, remove from running list: " + handle);
                break;
            }
        }

        /// <summary>
        /// Pauses the coroutine identified by the given handle.
        /// </summary>
        /// <param name="handle">The handle of the coroutine to pause.</param>
        public void PauseCoroutine(int handle) {
            foreach (var runner in RunnerList) {
                if (runner.TaskHandle != handle) {
                    continue;
                }
                runner.Pause();
                break;
            }
        }

        /// <summary>
        /// Resumes the coroutine identified by the given handle.
        /// </summary>
        /// <param name="handle">The handle of the coroutine to unpause.</param>
        public void UnPauseCoroutine(int handle) {
            foreach (var runner in RunnerList) {
                if (runner.TaskHandle != handle) {
                    continue;
                }
                runner.UnPause();
                break;
            }
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
        /// <param name="runnerId">The index of the target runner.</param>
        private void RunTask(CoroutineTask context, int runnerId) {
            context.RunnerId = runnerId;
            Debug.Log("CoroutinePool:RunTask - Run task, runnerId: " + context.RunnerId);
            var runner = GetOrSpawnRunner(runnerId);
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
                RunnerList.Add(runner);
                ++idx;
            }
            Debug.Log("CoroutinePool:GetOrSpawnRunner - spawning new runner: " + runnerId);
            return RunnerList[runnerId];
        }

        /// <summary>
        /// Searches for an idle runner or returns a new slot if capacity allows.
        /// </summary>
        /// <returns>The runner index, or -1 if all runners are busy and capacity is reached.</returns>
        private int FindIdleRunner() {
            foreach (var runner in RunnerList) {
                if (runner.IsStopped()) {
                    return runner.RunnerId;
                }
            }
            if (RunnerList.Count < Capacity) {
                return RunnerList.Count;
            }
            return -1;
        }

        /// <summary>
        /// Dequeues the next waiting task and runs it on an idle runner.
        /// </summary>
        /// <returns>True if a task was dispatched; false otherwise.</returns>
        private bool TryPopupAndRunNext() {
            if (TasksWaiting.Count <= 0) {
                return false;
            }

            var runnerId = FindIdleRunner();
            if (runnerId < 0) {
                return false;
            }

            var context = TasksWaiting[0];
            TasksWaiting.RemoveAt(0);

            Debug.Log("CoroutinePool:PopupAndRunNext - popup new task: " + context.Handle);
            RunTask(context, runnerId);
            return true;
        }

        /// <summary>
        /// Called each frame to dispatch waiting tasks to idle runners.
        /// </summary>
        internal void OnUpdate() {
            while (TryPopupAndRunNext()) {
                // nothing to do
            }
        }
    }
}