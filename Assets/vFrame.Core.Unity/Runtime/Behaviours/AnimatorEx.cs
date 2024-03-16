using UnityEngine;

namespace vFrame.Core.Behaviours
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorEx : MonoBehaviour
    {
        [SerializeField]
        private float _timeScale = 1f;

        [SerializeField]
        private float _speed = 1f;

        private Animator _animator;
        
        private void Awake() {
            _animator = GetComponent<Animator>();
        }

        public float TimeScale
        {
            get { return _timeScale; }
            set {
                _timeScale = value;
                ApplyState();
            }
        }

        public float Speed {
            get { return _speed; }
            set {
                _speed = value;
                ApplyState();
            }
        }

        private void ApplyState() {
            if (!_animator) {
                return;
            }
            _animator.speed = _speed * _timeScale;
        }
    }
}