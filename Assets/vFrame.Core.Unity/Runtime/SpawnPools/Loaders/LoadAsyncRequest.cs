// ------------------------------------------------------------
//         File: LoadAsyncRequest.cs
//        Brief: Base async request for loading a GameObject through a loader.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using UnityEngine;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Abstract base class for asynchronous GameObject load requests used by the spawn pool system.
    /// Subclasses implement <see cref="Validate"/> to supply the loaded <see cref="GameObject"/>.
    /// </summary>
    public abstract class LoadAsyncRequest : AsyncRequest, ILoadAsyncRequest
    {
        /// <summary>
        /// Gets or sets the loaded <see cref="GameObject"/>.
        /// </summary>
        public GameObject GameObject { get; set; }

        /// <summary>
        /// Clears the <see cref="GameObject"/> reference when the request is destroyed.
        /// </summary>
        protected override void OnDestroy() {
            GameObject = null;
            base.OnDestroy();
        }

        /// <summary>
        /// Called when the async request starts. Default implementation does nothing.
        /// </summary>
        protected override void OnStart() { }

        /// <summary>
        /// Called when the async request is stopped. Default implementation does nothing.
        /// </summary>
        protected override void OnStop() { }

        /// <summary>
        /// Polls the loader each frame via <see cref="Validate"/>;
        /// finishes when a valid <see cref="GameObject"/> is produced or aborts on null.
        /// </summary>
        protected override void OnUpdate() {
            if (!Validate(out var obj)) {
                return;
            }
            if (!obj) {
                Abort();
                return;
            }
            GameObject = obj;
            Finish();
        }

        /// <summary>
        /// Checks whether the loader has produced a valid <see cref="GameObject"/>.
        /// </summary>
        /// <param name="obj">Receives the loaded <see cref="GameObject"/> when available.</param>
        /// <returns><c>true</c> if the object is ready; otherwise <c>false</c>.</returns>
        protected abstract bool Validate(out GameObject obj);
    }
}
