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

namespace vFrame.Core.Coroutine
{
    public class CoroutinePool
    {
        private static int _index;

        private static GameObject _parent;
        private static bool _parentCreated;
        private static GameObject PoolParent
        {
            get
            {
                if (_parentCreated)
                    return _parent;
                
                _parent = new GameObject("CoroutinePools");
                Object.DontDestroyOnLoad(_parent);
                
                _parentCreated = true;
                return _parent;
            }
        }

        private readonly int _capacity;
        private readonly GameObject _holder;
        private readonly List<CoroutineBehaviour> _coroutineList;
        private int _taskHandle;
        
        private readonly Dictionary<int, CoroutineTask> _tasksRunning;
        private readonly LinkedList<CoroutineTask> _tasksWaiting;

        public CoroutinePool(string name = null, int capacity = int.MaxValue)
        {
            _capacity = capacity;
            _coroutineList = new List<CoroutineBehaviour>();
            
            _tasksRunning = new Dictionary<int, CoroutineTask>();
            _tasksWaiting = new LinkedList<CoroutineTask>();
            
            _holder = new GameObject(string.Format("Pool_{0}({1})", ++_index, name ?? "Unnamed"));
            _holder.transform.SetParent(PoolParent.transform);
        }

        public int StartCoroutine(IEnumerator task)
        {
            var handle = GenerateTaskHandle();

#if DEBUG_COROUTINE_POOL
            Debug.Log("CoroutinePool:StartCoroutine - handle: " + handle);
#endif
            
            var context = new CoroutineTask {Handle = handle, Task = task};

            if (_tasksRunning.Count < _capacity)
            {
#if DEBUG_COROUTINE_POOL
                Debug.Log("CoroutinePool:StartCoroutine - pool not full, start running task ..");
#endif
                RunTask(context);
            }
            else
            {
#if DEBUG_COROUTINE_POOL
                Debug.Log("CoroutinePool:StartCoroutine - pool is full, add to waiting list ..");
#endif
                _tasksWaiting.AddLast(context);
            }
            
            return handle;
        }

        public void StopCoroutine(int handle)
        {
#if DEBUG_COROUTINE_POOL
            Debug.Log("CoroutinePool:StopCoroutine - Stopping coroutine: " + handle);
#endif
            // Remove from waiting list
            foreach (var context in _tasksWaiting)
            {
                if (context.Handle != handle)
                    continue;
                _tasksWaiting.Remove(context);
#if DEBUG_COROUTINE_POOL
            Debug.Log("CoroutinePool:StopCoroutine - Stopping coroutine, remove from waiting list: " + handle);
#endif
                break;
            }
            
            // Remove from running list
            foreach (var kv in _tasksRunning)
            {
                if (kv.Key != handle)
                    continue;

                var runner = _coroutineList[kv.Value.RunnerId];
                runner.StopAllCoroutines();
                _tasksRunning.Remove(handle);
                
#if DEBUG_COROUTINE_POOL
            Debug.Log("CoroutinePool:StopCoroutine - Stopping coroutine, remove from running list: " + handle);
#endif
                break;
            }
        }

        private int GenerateTaskHandle()
        {
            return ++_taskHandle;
        }

        private void RunTask(CoroutineTask context)
        {
            context.RunnerId = FindIdleRunner();

#if DEBUG_COROUTINE_POOL
            Debug.Log("CoroutinePool:RunTask - Run task, runnerId: " + context.runnerId);
#endif
            
            var runner = GetOrSpawnRunner(context.RunnerId);
            runner.StartCoroutine(TaskProcessWrap(context));
            
            _tasksRunning.Add(context.Handle, context);
        }

        private CoroutineBehaviour GetOrSpawnRunner(int runnerId)
        {
            if (runnerId < _coroutineList.Count)
            {
#if DEBUG_COROUTINE_POOL
                Debug.Log("CoroutinePool:GetOrSpawnRunner - runner exist: " + runnerId);
#endif
                return _coroutineList[runnerId];
            }
                
            var runner = new GameObject("Coroutine_" + runnerId).AddComponent<CoroutineBehaviour>();
            runner.transform.SetParent(_holder.transform);
            _coroutineList.Add(runner);
            
#if DEBUG_COROUTINE_POOL
            Debug.Log("CoroutinePool:GetOrSpawnRunner - spawning new runner: " + runnerId);
#endif
            
            return runner;
        }

        private int FindIdleRunner()
        {
            for (var i = 0; i < _capacity; i++)
            {
                var running = false;
                foreach (var kv in _tasksRunning)
                {
                    if (kv.Value.RunnerId != i)
                        continue;
                    running = true;
                    break;
                }

                if (!running)
                    return i;
            }
            throw new System.IndexOutOfRangeException("No idling runner now.");
        }
        
        private IEnumerator TaskProcessWrap(CoroutineTask context)
        {
            // Avoid finishing immediately
            yield return null;
            
#if DEBUG_COROUTINE_POOL
            Debug.Log("CoroutinePool:TaskProcessWrap - task starting: " + context.task.GetHashCode());
#endif
            yield return context.Task;
            
#if DEBUG_COROUTINE_POOL
            Debug.Log("CoroutinePool:TaskProcessWrap - task finished: " + context.task.GetHashCode());
#endif
            
            _coroutineList[context.RunnerId].StopAllCoroutines();
            _tasksRunning.Remove(context.Handle);
            
            PopupAndRunNext();
        }

        private void PopupAndRunNext()
        {
            if (_tasksWaiting.Count <= 0)
                return;

            var context = _tasksWaiting.First.Value;
            _tasksWaiting.RemoveFirst();
            
#if DEBUG_COROUTINE_POOL
            Debug.Log("CoroutinePool:PopupAndRunNext - popup new task: " + context.handle);
#endif
            
            RunTask(context);
        }
    }
}