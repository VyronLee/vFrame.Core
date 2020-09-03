using System.Collections.Generic;
using UnityEngine;

namespace vFrame.Core.SpawnPools.Snapshots.Impls
{
    public class BehaviourSnapshot : Snapshot
    {
        private readonly Dictionary<int, bool> _behaviourEnables = new Dictionary<int, bool>(8);

        public override void Take() {
            var behaviours = Target.GetComponents<Behaviour>();
            foreach (var behaviour in behaviours) {
                var id = behaviour.GetInstanceID();
                _behaviourEnables[id] = behaviour.enabled;
            }
        }

        public override void Restore() {
            var behaviours = Target.GetComponents<Behaviour>();
            foreach (var behaviour in behaviours) {
                var id = behaviour.GetInstanceID();
                if (_behaviourEnables.TryGetValue(id, out var enable)) {
                    behaviour.enabled = enable;
                }
            }
        }
    }
}