using System.Collections;
using UnityEngine;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.SpawnPools.Loaders
{
    public class LoadAsyncRequestOnLoaded : LoadAsyncRequest
    {
        public static LoadAsyncRequestOnLoaded Create(GameObject obj) {
            var request = ObjectPool<LoadAsyncRequestOnLoaded>.Shared.Get();
            request.GameObject = obj;
            request.IsFinished = true;
            return request;
        }

        public override void Dispose() {
            ObjectPool<LoadAsyncRequestOnLoaded>.Shared.Return(this);
        }

        public override IEnumerator Await() {
            IsFinished = true;
            yield break;
        }
    }
}