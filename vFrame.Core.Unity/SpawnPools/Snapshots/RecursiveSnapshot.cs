using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using vFrame.Core.Base;
using vFrame.Core.Extensions.UnityEngine;

namespace vFrame.Core.SpawnPools.Snapshots
{
    public class RecursiveSnapshot : BaseObject<GameObject, IEnumerable<Type>>
    {
        protected readonly List<Type> EmptyList = new List<Type>();

        private GameObject _root;
        private Dictionary<GameObject, List<Snapshot>> _snapshots;

        private Action<Transform> _takeNodeSnapshot;
        private Action<Transform> _restoreNodeSnapshot;
        private IEnumerable<Type> _photographer;

        protected override void OnCreate(GameObject root, IEnumerable<Type> snapshots) {
            _root = root;
            _photographer = snapshots ?? EmptyList;

            _snapshots = new Dictionary<GameObject, List<Snapshot>>(32);

            _takeNodeSnapshot = TakeOneSnapshot;
            _restoreNodeSnapshot = RestoreOneSnapshot;
        }

        protected override void OnDestroy() {
            _snapshots?.Clear();
            _snapshots = null;
        }

        public void Take() {
            _root.transform.TravelSelfAndChildren(_takeNodeSnapshot);
        }

        public void Restore() {
            _root.transform.TravelSelfAndChildren(_restoreNodeSnapshot);
        }

        private void TakeOneSnapshot(Transform node) {
            _snapshots[node.gameObject] = new List<Snapshot>(_photographer.Count());

            foreach (var snapshotType in _photographer) {
                var snapshot = Activator.CreateInstance(snapshotType) as Snapshot;
                if (snapshot == null)
                    continue;

                snapshot.Create(node.gameObject);
                snapshot.Take();

                _snapshots[node.gameObject].Add(snapshot);
            }
        }

        private void RestoreOneSnapshot(Transform node) {
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
