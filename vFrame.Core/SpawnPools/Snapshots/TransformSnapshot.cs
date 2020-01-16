using UnityEngine;

namespace vFrame.Core.SpawnPools.Snapshots
{
    internal class TransformSnapshot : Snapshot
    {
        private Vector3 _localPosition;
        private Vector3 _localScale;
        private Quaternion _localRotation;

        public override void Take() {
            _localPosition = Target.transform.localPosition;
            _localScale = Target.transform.localScale;
            _localRotation = Target.transform.localRotation;
        }

        public override void Restore() {
            Target.transform.localPosition = _localPosition;
            Target.transform.localScale = _localScale;
            Target.transform.localRotation = _localRotation;
        }
    }
}