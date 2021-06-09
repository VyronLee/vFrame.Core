using System.Collections.Generic;
using UnityEngine;
using vFrame.Core.ObjectPools.Builtin;

namespace vFrame.Core.SpawnPools.Snapshots.Impls
{
    public class BehaviourSnapshot : Snapshot
    {
        private Dictionary<int, bool> _behaviourEnables;

        protected override void OnDestroy() {
            if (null != _behaviourEnables) {
                DictionaryPool<int, bool>.Shared.Return(_behaviourEnables);
            }
            _behaviourEnables = null;

            base.OnDestroy();
        }

        public override void Take() {
            var behaviours = ListPool<Behaviour>.Shared.Get();
            Target.GetComponents(behaviours);

            if (null == _behaviourEnables) {
                _behaviourEnables = DictionaryPool<int, bool>.Shared.Get();
            }

            foreach (var behaviour in behaviours) {
                if (!behaviour) {
                    continue;
                }
                var id = behaviour.GetInstanceID();
                _behaviourEnables[id] = behaviour.enabled;
            }

            ListPool<Behaviour>.Shared.Return(behaviours);
        }

        public override void Restore() {
            if (null == _behaviourEnables || _behaviourEnables.Count <= 0) {
                return;
            }

            var behaviours = ListPool<Behaviour>.Shared.Get();
            Target.GetComponents<Behaviour>();

            foreach (var behaviour in behaviours) {
                if (!behaviour) {
                    continue;
                }

                var id = behaviour.GetInstanceID();
                if (_behaviourEnables.TryGetValue(id, out var enable)) {
                    behaviour.enabled = enable;
                }
            }

            ListPool<Behaviour>.Shared.Return(behaviours);
        }
    }
}