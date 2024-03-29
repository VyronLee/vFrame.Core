//------------------------------------------------------------
//        File:  CoroutinePool.cs
//       Brief:  Coroutine pool.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-08 22:09
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using vFrame.Core.Exceptions;
using vFrame.Core.Loggers;
using vFrame.Core.Unity.Extensions;
using Object = UnityEngine.Object;
using Debug = vFrame.Core.Unity.Coroutine.CoroutinePoolDebug;

namespace vFrame.Core.Unity.Coroutine
{
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

        public CoroutinePool(string name = null, int capacity = int.MaxValue) {
            Capacity = capacity;
            RunnerList = new List<CoroutineRunnerBehaviour>();

            TasksWaiting = new List<CoroutineTask>(16);

            _holder = new GameObject($"Pool_{++_index}({name ?? "Unnamed"})");
            _holder.AddComponent<CoroutinePoolBehaviour>().Pool = this;
            _holder.transform.SetParent(PoolParent.transform);
        }

        private static GameObject PoolParent {
            get {
                if (_parent) {
                    return _parent;
                }
                return _parent = new GameObject("CoroutinePools").DontDestroyEx();
            }
        }

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
                Object.Destroy(_holder);
            }
        }

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

        public void StopCoroutine(int handle) {
            Debug.Log("CoroutinePool:StopCoroutine - Stopping coroutine: " + handle);
            // Remove from waiting list
            foreach (var context in TasksWaiting) {
                if (context.Handle != handle) {
                    continue;
                }
                TasksWaiting.Remove(context);
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

        public void PauseCoroutine(int handle) {
            foreach (var runner in RunnerList) {
                if (runner.TaskHandle != handle) {
                    continue;
                }
                runner.Pause();
                break;
            }
        }

        public void UnPauseCoroutine(int handle) {
            foreach (var runner in RunnerList) {
                if (runner.TaskHandle != handle) {
                    continue;
                }
                runner.UnPause();
                break;
            }
        }

        private int GenerateTaskHandle() {
            return ++_taskHandle;
        }

        private void RunTask(CoroutineTask context, int runnerId) {
            context.RunnerId = runnerId;
            Debug.Log("CoroutinePool:RunTask - Run task, runnerId: " + context.RunnerId);
            var runner = GetOrSpawnRunner(runnerId);
            runner.CoStart(context);
        }

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

        internal void OnUpdate() {
            while (TryPopupAndRunNext()) {
                // nothing to do
            }
        }
    }
}