using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vFrame.Core.SpawnPools.Loaders
{
    public abstract class LoadAsyncRequest : ILoaderAsyncRequest
    {
        public abstract void Dispose();

        protected GameObject GameObject { get; set; }
        public IEnumerable<Type> AdditionalSnapshotTypes { get; set; }
        public Action<GameObject, IEnumerable<Type>> OnLoadCallback { get; set; }

        public GameObject GetGameObject() {
            OnGetGameObject?.Invoke(GameObject);
            return GameObject;
        }

        public Action<GameObject> OnGetGameObject;

        public abstract IEnumerator Await();
        public bool IsFinished { get; protected set; }

        protected void InvokeLoadCallback() {
            OnLoadCallback?.Invoke(GameObject, AdditionalSnapshotTypes);
        }
    }
}