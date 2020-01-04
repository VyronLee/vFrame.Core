using UnityEngine;
using Logger = vFrame.Core.Loggers.Logger;

namespace vFrame.Core.SpawnPools.Behaviours
{
    public class PoolObjectIdentity : MonoBehaviour
    {
        [SerializeField] private string _assetPath;

        public string AssetPath {
            get => _assetPath;
            internal set => _assetPath = value;
        }

        private void OnDestroy() {
            Logger.Warning("Pool object({0}) get destroyed outside the pool!!!!", _assetPath);
        }
    }
}