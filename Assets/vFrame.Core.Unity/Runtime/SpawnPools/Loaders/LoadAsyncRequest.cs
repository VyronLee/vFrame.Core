using UnityEngine;
using vFrame.Core.Unity.Asynchronous;

namespace vFrame.Core.Unity.SpawnPools
{
    public abstract class LoadAsyncRequest : AsyncRequest, ILoadAsyncRequest
    {
        public GameObject GameObject { get; set; }

        protected override void OnDestroy() {
            GameObject = null;
            base.OnDestroy();
        }

        protected override void OnStart() { }

        protected override void OnStop() { }

        protected override void OnUpdate() {
            if (!Validate(out var obj)) {
                return;
            }
            if (!obj) {
                Abort();
                return;
            }
            GameObject = obj;
            Finish();
        }

        protected abstract bool Validate(out GameObject obj);
    }
}