using UnityEngine;

namespace vFrame.Core.Behaviours.Snapshots
{
    [AddComponentMenu("GameObjectSnapshot/ParticleSystemEnableState")]
    [DisallowMultipleComponent]
    public class ParticleSystemEnableStateSnapshot : GameObjectSnapshot
    {
        [SerializeField]
        private bool _enabled = false;

        public override bool Take() {
            var ps = transform.GetComponent<ParticleSystem>();
            if (!ps) {
                return false;
            }
            var emission = ps.emission;
            _enabled = emission.enabled;
            return true;
        }

        public override bool Restore() {
            var ps = transform.GetComponent<ParticleSystem>();
            if (!ps) {
                return false;
            }
            var emission = ps.emission;
            emission.enabled = _enabled;
            return true;
        }
    }
}