using System;
using System.Collections;
using UnityEngine;
using vFrame.Core.Unity.Asynchronous;

namespace vFrame.Core.Unity.SpawnPools
{
    public abstract class LoadAsyncRequest : AsyncRequest, ILoaderAsyncRequest
    {
        private bool _preprocessBeforeGet;
        internal Action<GameObject> OnGetGameObject;
        protected GameObject GameObject { get; set; }
        internal Action<GameObject> OnLoadCallback { get; set; }

        public GameObject GetGameObject() {
            ThrowIfDestroyed();

            if (!IsFinished) {
                throw new ObjectNotReadyException();
            }
            if (!_preprocessBeforeGet) {
                OnGetGameObject?.Invoke(GameObject);
            }
            _preprocessBeforeGet = true;

            return GameObject;
        }

        protected override void OnCreate() {
            Clear();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            Clear();
        }

        protected void Clear() {
            OnLoadCallback = null;
            OnGetGameObject = null;
            GameObject = null;

            _preprocessBeforeGet = false;
        }

        protected override IEnumerator OnProcess() {
            yield return OnProcessLoad();

            if (!GameObject) {
                throw new ObjectNotLoadedException();
            }
            OnLoadCallback?.Invoke(GameObject);
            IsFinished = true;
        }

        protected abstract IEnumerator OnProcessLoad();
    }
}