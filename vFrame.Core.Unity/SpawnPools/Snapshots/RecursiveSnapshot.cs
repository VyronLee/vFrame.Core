using System;
using System.Collections.Generic;
using UnityEngine;
using vFrame.Core.Base;
using vFrame.Core.Extensions.UnityEngine;
using vFrame.Core.ObjectPools.Builtin;

namespace vFrame.Core.SpawnPools.Snapshots
{
    public class RecursiveSnapshot : BaseObject<GameObject, IEnumerable<Type>>
    {
        protected static readonly List<Type> EmptyList = new List<Type>();

        private GameObject _root;
        private Dictionary<GameObject, List<Snapshot>> _snapshots;

        private Action<Transform> _takeNodeSnapshot;
        private Action<Transform> _restoreNodeSnapshot;
        private IEnumerable<Type> _photographer;

        protected override void OnCreate(GameObject root, IEnumerable<Type> snapshots) {
            _root = root;
            _photographer = snapshots ?? EmptyList;

            _takeNodeSnapshot = TakeOneSnapshot;
            _restoreNodeSnapshot = RestoreOneSnapshot;
        }

        protected override void OnDestroy() {
            Reset();

            _root = null;
            _photographer = null;
            _takeNodeSnapshot = null;
            _restoreNodeSnapshot = null;
        }

        public void Take() {
            Reset();

            if (null == _snapshots) {
                _snapshots = DictionaryPool<GameObject, List<Snapshot>>.Shared.Get();
            }
            _root.transform.TravelSelfAndChildren(_takeNodeSnapshot);
        }

        public void Restore() {
            _root.transform.TravelSelfAndChildren(_restoreNodeSnapshot);
        }

        private void Reset() {
            if (null == _snapshots) {
                return;
            }

            foreach (var kv in _snapshots) {
                ListPool<Snapshot>.Shared.Return(kv.Value);
            }
            DictionaryPool<GameObject, List<Snapshot>>.Shared.Return(_snapshots);
            _snapshots = null;
        }

        private void TakeOneSnapshot(Transform node) {
            _snapshots[node.gameObject] = ListPool<Snapshot>.Shared.Get();
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
