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
    public class GameObjectSnapshotBehaviour : MonoBehaviour
    {
        [SerializeField]
        private GameObjectSnapshotSettings _snapshotSettings;

        [SerializeField]
        [HideInInspector]
        private List<GameObjectSnapshot> _snapshots = new List<GameObjectSnapshot>();

        public GameObjectSnapshotSettings SnapshotSettings {
            get => _snapshotSettings;
            set => _snapshotSettings = value;
        }

        private Action<Type> _takeSnapshot;
        private Action<GameObjectSnapshot> _restoreSnapshot;

        private void Awake() {
            hideFlags = HideFlags.HideInInspector;
        }

        public void Take() {
            Clear();

            if (!_snapshotSettings || null == _snapshotSettings.SnapshotTypes) {
                return;
            }
            _snapshotSettings.SnapshotTypes.ForEach(_takeSnapshot ?? (_takeSnapshot = TakeInternal));
        }

        public void Restore() {
            _snapshots?.ForEach(_restoreSnapshot ?? (_restoreSnapshot = RestoreInternal));
        }

        public void Clear() {
            if (null == _snapshots) {
                return;
            }

            foreach (var snapshot in _snapshots) {
                snapshot.DestroyEx();
            }
            _snapshots.Clear();
        }

        private void TakeInternal(Type snapshotType) {
            if (null == snapshotType) {
                return;
            }

            if (!typeof(GameObjectSnapshot).IsAssignableFrom(snapshotType)) {
                Debug.LogError("Snapshot type must be inherit from GameObjectSnapshot: " + snapshotType.Name);
                return;
            }

            var snapshot = gameObject.GetComponent(snapshotType) as GameObjectSnapshot;
            if (!snapshot) {
                snapshot = AddComponentInternal(snapshotType) as GameObjectSnapshot;
            }

            if (!snapshot) {
                Debug.LogError("Add snapshot component failed, type: " + snapshotType.Name);
                return;
            }

            if (!snapshot.Take()) {
                snapshot.DestroyEx();
                return;
            }
            snapshot.hideFlags = HideFlags.HideInInspector;

            _snapshots.Add(snapshot);
        }

        private void RestoreInternal(GameObjectSnapshot snapshot) {
            if (!snapshot) {
                return;
            }
            snapshot.Restore();
        }

        private Component AddComponentInternal(Type type) {
            Component comp;
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                comp = Undo.AddComponent(gameObject, type);
                return comp;
            }
#endif
            comp = gameObject.AddComponent(type);
            return comp;
        }
    }
}