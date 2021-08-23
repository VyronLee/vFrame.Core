//------------------------------------------------------------
//        File:  CoroutinePool.cs
//       Brief:  Coroutine pool.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-09-08 22:09
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vFrame.Core.Loggers;
using Logger = vFrame.Core.Loggers.Logger;

namespace vFrame.Core.Coroutine
{
    public class CoroutinePool
    {
        private static int _index;

        private static GameObject _parent;
        private static bool _parentCreated;

        private static GameObject PoolParent {
            get {
                if (_parentCreated)
                    return _parent;

                _parent = new GameObject("CoroutinePools");
                Object.DontDestroyOnLoad(_parent);

                _parentCreated = true;
                return _parent;
            }
        }

        internal readonly int Capacity;
        internal readonly HashSet<int> IdleSlots;
        internal readonly List<CoroutineBehaviour> CoroutineList;
        internal readonly List<CoroutineTask> TasksRunning;
        internal readonly List<CoroutineTask> TasksWaiting;

        private readonly GameObject _holder;
        private int _taskHandle;
        private static readonly LogTag LogTag = new LogTag("CoroutinePool");

        public CoroutinePool(string name = null, int capacity = int.MaxValue) {
            Capacity = capacity;
            IdleSlots = new HashSet<int>();
            CoroutineList = new List<CoroutineBehaviour>();

            TasksRunning = new List<CoroutineTask>(16);
            TasksWaiting = new List<CoroutineTask>(16);

            _holder = new GameObject(string.Format("Pool_{0}({1})", ++_index, name ?? "Unnamed"));
            _holder.AddComponent<CoroutinePoolBehaviour>().Pool = this;
            _holder.transform.SetParent(PoolParent.transform);
        }

        public void Destroy() {
            foreach (var task in TasksRunning) {
                var runner = CoroutineList[task.RunnerId];
                if (!runner) {
                    continue;
                }
                runner.CoStop();
                Object.Destroy(runner.gameObject);
            }
            TasksRunning.Clear();
            TasksWaiting.Clear();

            if (_holder) {
                Object.Destroy(_holder);
            }
        }

        public int StartCoroutine(IEnumerator task) {
            var handle = GenerateTaskHandle();

#if DEBUG_COROUTINE_POOL
           Logger.Info(LogTag, "CoroutinePool:StartCoroutine - handle: " + handle);
#endif

            var context = new CoroutineTask {Handle = handle, Task = task};

            if (TasksRunning.Count < Capacity) {
#if DEBUG_COROUTINE_POOL
                Logger.Info(LogTag, "CoroutinePool:StartCoroutine - pool not full, start running task ..");
#endif
                RunTask(context);
            }
            else {
#if DEBUG_COROUTINE_POOL
                Logger.Info(LogTag, "CoroutinePool:StartCoroutine - pool is full, add to waiting list ..");
#endif
                TasksWaiting.Add(context);
            }

            return handle;
        }

        public void StopCoroutine(int handle) {
#if DEBUG_COROUTINE_POOL
            Logger.Info(LogTag, "CoroutinePool:StopCoroutine - Stopping coroutine: " + handle);
#endif
            // Remove from waiting list
            foreach (var context in TasksWaiting) {
                if (context.Handle != handle)
                    continue;
                TasksWaiting.Remove(context);
#if DEBUG_COROUTINE_POOL
                Logger.Info(LogTag, "CoroutinePool:StopCoroutine - Stopping coroutine, remove from waiting list: " + handle);
#endif
                break;
            }

            // Remove from running list
            foreach (var task in TasksRunning) {
                if (task.Handle != handle)
                    continue;

                var runner = CoroutineList[task.RunnerId];
                runner.CoStop();

                TasksRunning.Remove(task);

#if DEBUG_COROUTINE_POOL
                Logger.Info(LogTag, "CoroutinePool:StopCoroutine - Stopping coroutine, remove from running list: " + handle);
#endif
                break;
            }
        }

        public void PauseCoroutine(int handle) {
            foreach (var task in TasksRunning) {
                if (task.Handle != handle)
                    continue;

                var runner = CoroutineList[task.RunnerId];
                runner.Pause();
                break;
            }
        }

        public void UnPauseCoroutine(int handle) {
            foreach (var task in TasksRunning) {
                if (task.Handle != handle)
                    continue;

                var runner = CoroutineList[task.RunnerId];
                runner.UnPause();
                break;
            }
        }

        private int GenerateTaskHandle() {
            return ++_taskHandle;
        }

        private void RunTask(CoroutineTask context) {
            context.RunnerId = FindIdleRunner();

#if DEBUG_COROUTINE_POOL
            Logger.Info(LogTag, "CoroutinePool:RunTask - Run task, runnerId: " + context.RunnerId);
#endif

            var runner = GetOrSpawnRunner(context.RunnerId);
            runner.CoStart(TaskProcessWrap(context));

            if (runner.IsRunning()) {
                TasksRunning.Add(context);
            }
        }

        private CoroutineBehaviour GetOrSpawnRunner(int runnerId) {
            if (runnerId < CoroutineList.Count) {
#if DEBUG_COROUTINE_POOL
                Logger.Info(LogTag, "CoroutinePool:GetOrSpawnRunner - runner exist: " + runnerId);
#endif
                return CoroutineList[runnerId];
            }

            var runner = new GameObject("Coroutine_" + runnerId).AddComponent<CoroutineBehaviour>();
            runner.RunnerId = runnerId;
            runner.OnFinished = OnTaskFinished;
            runner.transform.SetParent(_holder.transform);

            CoroutineList.Add(runner);

#if DEBUG_COROUTINE_POOL
            Logger.Info(LogTag, "CoroutinePool:GetOrSpawnRunner - spawning new runner: " + runnerId);
#endif

            return runner;
        }

        private int FindIdleRunner() {
            if (IdleSlots.Count > 0) {
                foreach (var slot in IdleSlots) {
                    IdleSlots.Remove(slot);
                    return slot;
                }
            }

            if (TasksRunning.Count < Capacity) {
                return TasksRunning.Count;
            }

            throw new System.IndexOutOfRangeException("No idling runner now.");
        }

        private IEnumerator TaskProcessWrap(CoroutineTask context) {
#if DEBUG_COROUTINE_POOL
            Logger.Info(LogTag, "CoroutinePool:TaskProcessWrap - task starting: " + context.Task.GetHashCode());
#endif
            yield return context.Task;

#if DEBUG_COROUTINE_POOL
            Logger.Info(LogTag, "CoroutinePool:TaskProcessWrap - task finished: " + context.Task.GetHashCode());
#endif
            TasksRunning.Remove(context);
        }

        private void OnTaskFinished(int runnerId) {
            IdleSlots.Add(runnerId);
            PopupAndRunNext();
        }

        private void PopupAndRunNext() {
            if (TasksWaiting.Count <= 0)
                return;

            var context = TasksWaiting[0];
            TasksWaiting.RemoveAt(0);

#if DEBUG_COROUTINE_POOL
            Logger.Info(LogTag, "CoroutinePool:PopupAndRunNext - popup new task: " + context.Handle);
#endif

            RunTask(context);
        }
    }
}