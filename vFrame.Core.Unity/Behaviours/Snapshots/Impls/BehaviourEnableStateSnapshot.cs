﻿using System;
using UnityEngine;

namespace vFrame.Core.Behaviours.Snapshots
{
    [AddComponentMenu("GameObjectSnapshot/BehaviourEnableState")]
    [DisallowMultipleComponent]
    public class BehaviourEnableStateSnapshot : GameObjectSnapshot
    {
        [SerializeField]
        private BehaviourStateDict _states = new BehaviourStateDict();

        private Action<Behaviour> _takeInternal;

        public override bool Take() {
            _states.Clear();
            _takeInternal = _takeInternal ?? (_takeInternal = TakeInternal);
            var components = GetComponentsInChildren<Behaviour>(true);
            foreach (var comp in components) {
                _takeInternal(comp);
            }
            return _states.Count > 0;
        }

        public override bool Restore() {
            var ret = false;
            foreach (var kv in _states) {
                if (!kv.Key) {
                    continue;
                }
                kv.Key.enabled = kv.Value;
                ret = true;
            }
            return ret;
        }

        private void TakeInternal(Behaviour obj) {
            if (obj is GameObjectSnapshot
                || obj is GameObjectSnapshotBehaviour
                || obj is GameObjectSnapshotRecursiveBehaviour) {
                return;
            }
            _states[obj] = obj.enabled;
        }

        [Serializable]
        private class BehaviourStateDict : SerializableDictionary<Behaviour, bool>
        {

        }
    }
}