// ------------------------------------------------------------
//         File: PoolObjectIdentity.cs
//        Brief: MonoBehaviour that tracks identity and pooling state for pooled GameObjects.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using UnityEngine;

namespace vFrame.Core.Unity
{
    public class PoolObjectIdentity : MonoBehaviour
    {
        [SerializeField]
        private string _assetPath;

        [SerializeField]
        private bool _pooling;

        [SerializeField]
        private int _uniqueId;

        /// <summary>
        /// Gets or sets the resource path of the pooled asset.
        /// </summary>
        public string AssetPath {
            get => _assetPath;
            internal set => _assetPath = value;
        }

        /// <summary>
        /// Gets or sets whether this object is currently managed by a spawn pool.
        /// </summary>
        public bool IsPooling {
            get => _pooling;
            internal set => _pooling = value;
        }

        /// <summary>
        /// Gets or sets the unique identifier assigned by the spawn pool.
        /// </summary>
        public int UniqueId {
            get => _uniqueId;
            internal set => _uniqueId = value;
        }

        /// <summary>
        /// Gets or sets whether the underlying Unity object has been destroyed.
        /// </summary>
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