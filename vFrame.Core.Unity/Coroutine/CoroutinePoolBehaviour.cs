using System.Collections.Generic;
using UnityEngine;

namespace vFrame.Core.Coroutine
{
    internal class CoroutinePoolBehaviour : MonoBehaviour
    {
        [SerializeField]
        private int _capacity;
        [SerializeField]
        private List<CoroutineTask> _tasksRunning;
        [SerializeField]
        private List<CoroutineTask> _tasksWaiting;
        [SerializeField]
        private List<CoroutineRunnerBehaviour> _coroutineList;

        private CoroutinePool _pool;

        public CoroutinePool Pool {
            set {
                _capacity = value.Capacity;
                _coroutineList = value.CoroutineList;
                _tasksRunning = value.TasksRunning;
                _tasksWaiting = value.TasksWaiting;
                _pool = value;
            }
            get => _pool;
        }

        private void Update() {
            Pool?.OnUpdate();
        }
    }
}