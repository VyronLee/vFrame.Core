// ------------------------------------------------------------
//         File: DefaultGameObjectLoader.cs
//        Brief: Synchronous and asynchronous GameObject loader using Unity Resources API.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-02-18 15:04:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using UnityEngine;
using vFrame.Core;
using Object = UnityEngine.Object;

namespace vFrame.Core.Unity
{
    internal class DefaultGameObjectLoader : BaseObject<string>, IGameObjectLoader
    {
        private string _path;

        /// <summary>
        /// Synchronously loads a GameObject from the configured resource path.
        /// </summary>
        /// <returns>An instantiated copy of the loaded prefab.</returns>
        /// <exception cref="AssetLoadFailedException">Thrown when the prefab cannot be found at the resource path.</exception>
        public GameObject Load() {
            var prefab = Resources.Load<GameObject>(_path);
            if (!prefab) {
                throw new AssetLoadFailedException(_path);
            }
            return UnityEngine.Object.Instantiate(prefab);
        }

        /// <summary>
        /// Creates an asynchronous load request for the configured resource path.
        /// </summary>
        /// <returns>A <see cref="LoadAsyncRequest"/> that completes when the asset is loaded.</returns>
        public LoadAsyncRequest LoadAsync() {
            var request = new DefaultLoadAsyncRequest();
            request.Path = _path;
            return request;
        }

        /// <summary>
        /// Stores the resource path provided during creation.
        /// </summary>
        /// <param name="arg1">The resource path to load assets from.</param>
        protected override void OnCreate(string arg1) {
            _path = arg1;
        }

        /// <summary>
        /// Clears the stored resource path on destruction.
        /// </summary>
        protected override void OnDestroy() {
            _path = null;
        }

        private class DefaultLoadAsyncRequest : LoadAsyncRequest
        {
            private ResourceRequest _request;
            internal string Path { get; set; }
            public override float Progress => _request?.progress ?? 0;

            /// <summary>
            /// Cleans up the internal resource request and path on destruction.
            /// </summary>
            protected override void OnDestroy() {
                _request = null;
                Path = null;
                base.OnDestroy();
            }

            /// <summary>
            /// Validates whether the asynchronous resource load has completed.
            /// </summary>
            /// <param name="obj">When this method returns <c>true</c>, contains the instantiated GameObject; otherwise <c>null</c>.</param>
            /// <returns><c>true</c> if loading is complete and the object is ready; <c>false</c> if still loading.</returns>
            /// <exception cref="AssetLoadFailedException">Thrown when the loaded asset is not a valid GameObject.</exception>
            protected override bool Validate(out GameObject obj) {
                if (null == _request) {
                    _request = Resources.LoadAsync<GameObject>(Path);
                }
                if (!_request.isDone) {
                    obj = null;
                    return false;
                }

                var prefab = _request.asset as GameObject;
                if (!prefab) {
                    throw new AssetLoadFailedException(Path);
                }
                obj = UnityEngine.Object.Instantiate(prefab);
                return true;
            }
        }
    }
}