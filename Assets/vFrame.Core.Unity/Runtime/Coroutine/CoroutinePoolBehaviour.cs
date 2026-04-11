// ------------------------------------------------------------
//         File: CoroutinePoolBehaviour.cs
//        Brief: MonoBehaviour host that owns the coroutine pool and
//               drives its per-frame update tick.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 22:09:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace vFrame.Core.Unity
{
    internal class CoroutinePoolBehaviour : MonoBehaviour
    {
        [SerializeField]
        private int _capacity;

        [SerializeField]
        private Queue<CoroutineTask> _tasksWaiting;

        [SerializeField]
        private List<CoroutineRunnerBehaviour> _coroutineList;

        private CoroutinePool _pool;

        /// <summary>
        /// Gets or sets the coroutine pool instance bound to this behaviour.
        /// The setter synchronizes serialized fields for inspector display.
        /// </summary>
        public CoroutinePool Pool {
            set {
                _capacity = value.Capacity;
                _coroutineList = value.RunnerList;
                _tasksWaiting = value.TasksWaiting;
                _pool = value;
            }
            get => _pool;
        }

        /// <summary>
        /// Unity lifecycle callback that drives the pool's per-frame logic.
        /// </summary>
        private void Update() {
            Pool?.OnUpdate();
        }
    }
}