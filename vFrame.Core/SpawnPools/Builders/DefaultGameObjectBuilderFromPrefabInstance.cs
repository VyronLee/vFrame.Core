using System;
using System.Collections;
using UnityEngine;
using vFrame.Core.Base;
using Object = UnityEngine.Object;

namespace vFrame.Core.SpawnPools.Builders
{
    internal class DefaultGameObjectBuilderFromPrefabInstance : BaseObject<GameObject>, IGameObjectBuilderFromPrefabInstance
    {
        private GameObject _prefab;

        protected override void OnDestroy() {
            _prefab = null;
        }

        protected override void OnCreate(GameObject arg1) {
            _prefab = arg1;
        }

        public GameObject Spawn() {
            return Object.Instantiate(_prefab);
        }

        public IEnumerator SpawnAsync(Action<GameObject> callback) {
            callback(Object.Instantiate(_prefab));
            yield return null;
        }
    }
}