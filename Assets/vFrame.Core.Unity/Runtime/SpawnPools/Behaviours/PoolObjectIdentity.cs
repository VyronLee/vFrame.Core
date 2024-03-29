using UnityEngine;
using Logger = vFrame.Core.Loggers.Logger;

namespace vFrame.Core.Unity.SpawnPools
{
    public class PoolObjectIdentity : MonoBehaviour
    {
        [SerializeField]
        private string _assetPath;

        [SerializeField]
        private bool _pooling;

        [SerializeField]
        private int _uniqueId;

        public string AssetPath {
            get => _assetPath;
            internal set => _assetPath = value;
        }

        public bool IsPooling {
            get => _pooling;
            internal set => _pooling = value;
        }

        public int UniqueId {
            get => _uniqueId;
            internal set => _uniqueId = value;
        }

        internal bool Destroyed { get; set; }

        private void Awake() {
            hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInInspector;
        }

        private void OnDestroy() {
            if (IsPooling && !Destroyed) {
                Logger.Warning("Pool object(id: {0}, path: {1}) get destroyed outside the pool!!!!",
                    _uniqueId, _assetPath);
            }
        }
    }
}