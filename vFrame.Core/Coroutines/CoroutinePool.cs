using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vFrame.Core.Coroutines
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
        private readonly List<CoroutineBehaviour> _coroutines;
        private int _taskHandle;
        
        private readonly Dictionary<int, CoroutineTask> _tasksRunning;
        private readonly LinkedList<CoroutineTask> _tasksWaiting;

        public CoroutinePool(string name = null, int capacity = int.MaxValue)
        {
            _capacity = capacity;
            _coroutines = new List<CoroutineBehaviour>();
            
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
            
            var context = new CoroutineTask {handle = handle, task = task};

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
                if (context.handle != handle)
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

                var runner = _coroutines[kv.Value.runnerId];
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
            context.runnerId = FindIdleRunner();

#if DEBUG_COROUTINE_POOL
            Debug.Log("CoroutinePool:RunTask - Run task, runnerId: " + context.runnerId);
#endif
            
            var runner = GetOrSpawnRunner(context.runnerId);
            runner.StartCoroutine(TaskProcessWrap(context));
            
            _tasksRunning.Add(context.handle, context);
        }

        private CoroutineBehaviour GetOrSpawnRunner(int runnerId)
        {
            if (runnerId < _coroutines.Count)
            {
#if DEBUG_COROUTINE_POOL
                Debug.Log("CoroutinePool:GetOrSpawnRunner - runner exist: " + runnerId);
#endif
                return _coroutines[runnerId];
            }
                
            var runner = new GameObject("Coroutine_" + runnerId).AddComponent<CoroutineBehaviour>();
            runner.transform.SetParent(_holder.transform);
            _coroutines.Add(runner);
            
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
                    if (kv.Value.runnerId != i)
                        continue;
                    running = true;
                    break;
                }

                if (!running)
                    return i;
            }
            throw new System.InvalidOperationException("No idling runner now.");
        }
        
        private IEnumerator TaskProcessWrap(CoroutineTask context)
        {
            // Avoid finishing immediately
            yield return null;
            
#if DEBUG_COROUTINE_POOL
            Debug.Log("CoroutinePool:TaskProcessWrap - task starting: " + context.task.GetHashCode());
#endif
            yield return context.task;
            
#if DEBUG_COROUTINE_POOL
            Debug.Log("CoroutinePool:TaskProcessWrap - task finished: " + context.task.GetHashCode());
#endif
            
            _coroutines[context.runnerId].StopAllCoroutines();
            _tasksRunning.Remove(context.handle);
            
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