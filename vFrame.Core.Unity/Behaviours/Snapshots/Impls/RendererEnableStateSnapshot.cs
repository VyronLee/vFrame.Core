using UnityEngine;

namespace vFrame.Core.Behaviours.Snapshots
{
    [AddComponentMenu("GameObjectSnapshot/RendererEnableState")]
    [DisallowMultipleComponent]
    public class RendererEnableStateSnapshot : GameObjectSnapshot
    {
        [SerializeField]
        private bool _enabled = false;

        public override bool Take() {
            var renderer_ = GetComponent<Renderer>();
            if (!renderer_) {
                return false;
            }
            _enabled = renderer_.enabled;
            return true;
        }

        public override bool Restore() {
            var renderer_ = GetComponent<Renderer>();
            if (!renderer_) {
                return false;
            }
            renderer_.enabled = _enabled;
            return true;
        }
    }
}