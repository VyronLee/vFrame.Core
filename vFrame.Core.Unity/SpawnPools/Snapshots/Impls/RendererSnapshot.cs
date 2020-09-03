using UnityEngine;

namespace vFrame.Core.SpawnPools.Snapshots.Impls
{
    public class RendererSnapshot : Snapshot
    {
        private bool _rendererEnable;

        public override void Take() {
            var renderer = Target.GetComponent<Renderer>();
            if (!renderer) {
                return;
            }
            _rendererEnable = renderer.enabled;
        }

        public override void Restore() {
            var renderer = Target.GetComponent<Renderer>();
            if (!renderer) {
                return;
            }
            renderer.enabled = _rendererEnable;
        }
    }
}