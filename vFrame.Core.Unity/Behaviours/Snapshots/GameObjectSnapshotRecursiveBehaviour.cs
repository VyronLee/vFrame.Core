using System;
using System.Collections.Generic;
using UnityEngine;
using vFrame.Core.Extensions.UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace vFrame.Core.Behaviours.Snapshots
{
    [DisallowMultipleComponent]
    public class GameObjectSnapshotRecursiveBehaviour : MonoBehaviour
    {
        [SerializeField]
        private GameObjectSnapshotSettings _snapshotSettings;

        [SerializeField]
        private List<GameObjectSnapshotBehaviour> _snapshots = new List<GameObjectSnapshotBehaviour>();

        public GameObjectSnapshotSettings SnapshotSettings {
            get => _snapshotSettings;
            set => _snapshotSettings = value;
        }

        private Action<Transform> _takeInternal;

        private void Awake() {
            if (null == _snapshots)
                return;
            foreach (var snapshot in _snapshots) {
                snapshot.hideFlags |= HideFlags.HideInInspector;
            }
        }

        public void Take() {
            _snapshots.Clear();

            if (!_snapshotSettings) {
                return;
            }
            transform.TravelSelfAndChildren<Transform>(_takeInternal ?? (_takeInternal = TakeInternal));
        }

        public void Restore() {
            foreach (var snapshot in _snapshots) {
                if (!snapshot) {
                    continue;
                }
                snapshot.Restore();
            }
        }

        private void TakeInternal(Transform target) {
            var snapshot = target.GetComponent<GameObjectSnapshotBehaviour>();
            if (!snapshot) {
                snapshot = AddComponentInternal(target.gameObject, typeof(GameObjectSnapshotBehaviour)) as GameObjectSnapshotBehaviour;
            }

            if (!snapshot) {
                Debug.LogError("Add snapshot behaviour component failed.");
                return;
            }

            snapshot.SnapshotSettings = _snapshotSettings;
            snapshot.Take();
            snapshot.hideFlags = HideFlags.HideInInspector;

            _snapshots.Add(snapshot);
        }

        private Component AddComponentInternal(GameObject target, Type type) {
            Component comp;
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                comp = Undo.AddComponent(target, type);
                return comp;
            }
#endif
            comp = target.AddComponent(type);
            return comp;
        }
    }
}