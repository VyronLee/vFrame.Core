using UnityEngine;

namespace vFrame.Core.Behaviours
{
    [RequireComponent(typeof(TrailRenderer))]
    public class TrailRendererEx : MonoBehaviour
    {
        private TrailRenderer _trailRender;
        private float _pauseTime;
        private float _trailTime;
        private bool _paused;

        [SerializeField]
        private float _timeScale = 1f;

        private void Awake() {
            _trailRender = GetComponent<TrailRenderer>();
            _trailTime = _trailRender.time;
        }

        public void Pause() {
            _pauseTime = Time.time;
            _trailRender.time = Mathf.Infinity;
            _paused = true;
        }

        public void UnPause() {
            var resumeTime = Time.time;
            _trailRender.time = resumeTime - _pauseTime + _trailTime / _timeScale;
            _paused = false;

            Invoke(nameof(DelayResetFromPause), _trailTime / _timeScale);
        }

        public bool IsPaused() {
            return _paused;
        }

        public float TimeScale {
            get => _timeScale;
            set {
                _timeScale = value;

                Pause();
                UnPause();
            }
        }

        private void DelayResetFromPause() {
            if (_paused) {
                return;
            }
            _trailRender.time = _trailTime / _timeScale;
        }
    }
}