using UnityEngine;

namespace vFrame.Core.Behaviours.Snapshots
{
    [AddComponentMenu("GameObjectSnapshot/Transform")]
    [DisallowMultipleComponent]
    public class TransformSnapshot : GameObjectSnapshot
    {
        [SerializeField]
        private bool _active;

        [SerializeField]
        private int _layer;

        [SerializeField]
        private string _tag;

        [SerializeField]
        private Vector3 _localPosition;

        [SerializeField]
        private Vector3 _localScale;

        [SerializeField]
        private Quaternion _localRotation;

        public override bool Take() {
            var gameObject_ = gameObject;
            _active = gameObject_.activeSelf;
            _layer = gameObject_.layer;
            _tag = gameObject_.tag;

            var transform_= transform;
            _localPosition = transform_.localPosition;
            _localScale = transform_.localScale;
            _localRotation = transform_.localRotation;
            return true;
        }

        public override bool Restore() {
            var gameObject_ = gameObject;
            gameObject_.SetActive(_active);
            gameObject_.layer = _layer;
            gameObject_.tag = _tag;

            var transform_= transform;
            transform_.localPosition = _localPosition;
            transform_.localScale = _localScale;
            transform_.localRotation = _localRotation;
            return true;
        }
    }
}