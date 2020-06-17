using System;
using System.Collections.Generic;
using UnityEngine;
using vFrame.Core.Base;
using vFrame.Core.Extensions.UnityEngine;

namespace vFrame.Core.SpawnPools.Snapshots
{
    internal class PoolObjectSnapshot : BaseObject<GameObject>
    {
        private static readonly List<Type> SnapshotTypes = new List<Type> {
            typeof(TransformSnapshot),
            typeof(GameObjectSnapshot),
            typeof(RendererSnapshot),
            typeof(BehaviourSnapshot),
        };

        private GameObject _target;
        private Dictionary<GameObject, List<Snapshot>> _snapshots;

        private Action<Transform> _takeNodeSnapshot;
        private Action<Transform> _restoreNodeSnapshot;

        protected override void OnCreate(GameObject arg1) {
            _target = arg1;
            _snapshots = new Dictionary<GameObject, List<Snapshot>>(32);

            _takeNodeSnapshot = TakeNodeSnapshot;
            _restoreNodeSnapshot = RestoreNodeSnapshot;
        }

        protected override void OnDestroy() {
            _snapshots.Clear();
            _snapshots = null;
        }

        public void Take() {
            _target.transform.TravelSelfAndChildren(_takeNodeSnapshot);
        }

        public void Restore() {
            _target.transform.TravelSelfAndChildren(_restoreNodeSnapshot);
        }

        private void TakeNodeSnapshot(Transform node) {
            _snapshots[node.gameObject] = new List<Snapshot>(SnapshotTypes.Count);

            foreach (var snapshotType in SnapshotTypes) {
                var snapshot = Activator.CreateInstance(snapshotType) as Snapshot;
                if (snapshot == null)
                    continue;

                snapshot.Create(node.gameObject);
                snapshot.Take();

                _snapshots[node.gameObject].Add(snapshot);
            }
        }

        private void RestoreNodeSnapshot(Transform node) {
            if (!node) {
                return;
            }

            if (!_snapshots.TryGetValue(node.gameObject, out var snapshots)) {
                return;
            }

            foreach (var snapshot in snapshots) {
                snapshot.Restore();
            }
        }
    }
}