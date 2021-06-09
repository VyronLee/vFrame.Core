using System;
using System.Collections.Generic;
using UnityEngine;
using vFrame.Core.ObjectPools.Builtin;
using vFrame.Core.SpawnPools.Snapshots;
using vFrame.Core.SpawnPools.Snapshots.Impls;

namespace vFrame.Core.SpawnPools
{
    internal class PoolObjectSnapshot : RecursiveSnapshot
    {
        private static readonly List<Type> DefaultSnapshotTypes = new List<Type> {
            typeof(TransformSnapshot),
            typeof(GameObjectSnapshot),
            typeof(RendererSnapshot),
            typeof(BehaviourSnapshot)
        };

        private List<Type> _snapshotTypes;

        protected override void OnCreate(GameObject target, IEnumerable<Type> additional) {
            _snapshotTypes = ListPool<Type>.Shared.Get();
            _snapshotTypes.AddRange(DefaultSnapshotTypes);
            _snapshotTypes.AddRange(additional ?? EmptyList);

            base.OnCreate(target, _snapshotTypes);
        }

        protected override void OnDestroy() {
            if (null != _snapshotTypes) {
                ListPool<Type>.Shared.Return(_snapshotTypes);
            }
            _snapshotTypes = null;

            base.OnDestroy();
        }
    }
}