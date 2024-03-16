using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vFrame.Core.Behaviours
{
    public class MethodInvoker : MonoBehaviour
    {
        [SerializeField]
        private float timeScale = 1;

        private void Awake() {
            hideFlags = HideFlags.DontSave | HideFlags.HideInInspector | HideFlags.NotEditable;
        }

        private void OnDestroy() {
            Stop();
        }

        private void OnDisable() {
            Stop();
        }

        public void Stop() {
            StopAllCoroutines();
        }

        public float TimeScale {
            get => timeScale;
            set => timeScale = value;
        }

        public UnityEngine.Coroutine Invoke(Func<IEnumerator> action) {
            return StartCoroutine(InternalInvoke(action));
        }

        public UnityEngine.Coroutine Invoke<T1>(Func<T1, IEnumerator> action, T1 param) {
            return StartCoroutine(InternalInvoke(action, param));
        }

        public UnityEngine.Coroutine Invoke<T1, T2>(Func<T1, T2, IEnumerator> action, T1 param1, T2 param2) {
            return StartCoroutine(InternalInvoke(action, param1, param2));
        }

        public UnityEngine.Coroutine Invoke<T1, T2, T3>(Func<T1, T2, T3, IEnumerator> action, T1 param1, T2 param2, T3 param3) {
            return StartCoroutine(InternalInvoke(action, param1, param2, param3));
        }

        public UnityEngine.Coroutine DelayInvoke(float time, Action action) {
            return StartCoroutine(InternalDelayInvoke(time, action));
        }

        public UnityEngine.Coroutine DelayInvoke<T1>(float time, Action<T1> action, T1 param) {
            return StartCoroutine(InternalDelayInvoke(time, action, param));
        }

        public UnityEngine.Coroutine DelayInvoke<T1, T2>(float time, Action<T1, T2> action, T1 param1, T2 param2) {
            return StartCoroutine(InternalDelayInvoke(time, action, param1, param2));
        }

        public UnityEngine.Coroutine DelayInvoke<T1, T2, T3>(float time, Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3) {
            return StartCoroutine(InternalDelayInvoke(time, action, param1, param2, param3));
        }

        public UnityEngine.Coroutine LoopInvoke(float interval, Func<bool> action, bool invokeImmediately = true) {
            return StartCoroutine(InternalLoopInvoke(interval, action, invokeImmediately));
        }

        public UnityEngine.Coroutine LoopInvoke<T1>(float interval, Func<T1, bool> action, T1 param1, bool invokeImmediately = true) {
            return StartCoroutine(InternalLoopInvoke(interval, action, param1, invokeImmediately));
        }

        public UnityEngine.Coroutine LoopInvoke<T1, T2>(float interval, Func<T1, T2, bool> action, T1 param1, T2 param2, bool invokeImmediately = true) {
            return StartCoroutine(InternalLoopInvoke(interval, action, param1, param2, invokeImmediately));
        }

        public UnityEngine.Coroutine LoopInvoke<T1, T2, T3>(float interval, Func<T1, T2, T3, bool> action, T1 param1, T2 param2, T3 param3, bool invokeImmediately = true) {
            return StartCoroutine(InternalLoopInvoke(interval, action, param1, param2, param3, invokeImmediately));
        }

        //==============================================//
        //                   Internal                   //
        //==============================================//

        private static IEnumerator InternalInvoke(Func<IEnumerator> action) {
            return action.Invoke();
        }

        private static IEnumerator InternalInvoke<T1>(Func<T1, IEnumerator> action, T1 param) {
            return action.Invoke(param);
        }

        private static IEnumerator InternalInvoke<T1, T2>(Func<T1, T2, IEnumerator> action, T1 param1, T2 param2) {
            return action.Invoke(param1, param2);
        }

        private static IEnumerator InternalInvoke<T1, T2, T3>(Func<T1, T2, T3, IEnumerator> action, T1 param1, T2 param2, T3 param3) {
            return action.Invoke(param1, param2, param3);
        }

        private IEnumerator<float> InternalDelayInvoke(float time, Action action) {
            while (time > 0) {
                yield return time -= Time.deltaTime * TimeScale;
            }
            action.Invoke();
        }

        private IEnumerator<float> InternalDelayInvoke<T1>(float time, Action<T1> action, T1 param) {
            while (time > 0) {
                yield return time -= Time.deltaTime * TimeScale;
            }
            action.Invoke(param);
        }

        private IEnumerator<float> InternalDelayInvoke<T1, T2>(float time, Action<T1, T2> action, T1 param1, T2 param2) {
            while (time > 0) {
                yield return time -= Time.deltaTime * TimeScale;
            }
            action.Invoke(param1, param2);
        }

        private IEnumerator<float> InternalDelayInvoke<T1, T2, T3>(float time, Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3) {
            while (time > 0) {
                yield return time -= Time.deltaTime * TimeScale;
            }
            action.Invoke(param1, param2, param3);
        }

        private IEnumerator<float> InternalLoopInvoke(float interval, Func<bool> action, bool invokeImmediately) {
            if (invokeImmediately) {
                if (action()) {
                    yield break;
                }
            }
            while (true) {
                var time = interval;
                while (time > 0) {
                    yield return time -= Time.deltaTime * TimeScale;
                }

                if (action()) {
                    break;
                }
            }
        }

        private IEnumerator<float> InternalLoopInvoke<T1>(float interval, Func<T1, bool> action, T1 param1, bool invokeImmediately) {
            if (invokeImmediately) {
                if (action(param1)) {
                    yield break;
                }
            }
            while (true) {
                var time = interval;
                while (time > 0) {
                    yield return time -= Time.deltaTime * TimeScale;
                }

                if (action(param1)) {
                    break;
                }
            }
        }

        private IEnumerator<float> InternalLoopInvoke<T1, T2>(float interval, Func<T1, T2, bool> action, T1 param1, T2 param2, bool invokeImmediately) {
            if (invokeImmediately) {
                if (action(param1, param2)) {
                    yield break;
                }
            }
            while (true) {
                var time = interval;
                while (time > 0) {
                    yield return time -= Time.deltaTime * TimeScale;
                }

                if (action(param1, param2)) {
                    break;
                }
            }
        }

        private IEnumerator<float> InternalLoopInvoke<T1, T2, T3>(float interval, Func<T1, T2, T3, bool> action, T1 param1, T2 param2, T3 param3, bool invokeImmediately) {
            if (invokeImmediately) {
                if (action(param1, param2, param3)) {
                    yield break;
                }
            }
            while (true) {
                var time = interval;
                while (time > 0) {
                    yield return time -= Time.deltaTime * TimeScale;
                }

                if (action(param1, param2, param3)) {
                    break;
                }
            }
        }
    }
}