using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

        protected override void OnCreate(GameObject target, IEnumerable<Type> additional) {
            var snapshotTypes = DefaultSnapshotTypes.ToList();
            snapshotTypes.AddRange(additional ?? EmptyList);

            base.OnCreate(target, snapshotTypes);
        }
    }
}