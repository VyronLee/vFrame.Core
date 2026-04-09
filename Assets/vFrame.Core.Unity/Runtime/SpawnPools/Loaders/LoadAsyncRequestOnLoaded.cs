// ------------------------------------------------------------
//         File: LoadAsyncRequestOnLoaded.cs
//        Brief: LoadAsyncRequest specialization for already-loaded assets.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2021-03-22 15:51:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using UnityEngine;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// A <see cref="LoadAsyncRequest"/> that immediately succeeds when the
    /// <see cref="LoadAsyncRequest.GameObject"/> reference is already available.
    /// </summary>
    public class LoadAsyncRequestOnLoaded : LoadAsyncRequest
    {
        /// <summary>
        /// Gets the load progress. Returns 1 when done, 0 otherwise, since the asset is already loaded.
        /// </summary>
        public override float Progress => IsDone ? 1f : 0f;

        /// <summary>
        /// Validates the load by returning the already-assigned <see cref="LoadAsyncRequest.GameObject"/>.
        /// </summary>
        /// <param name="obj">Receives the existing <see cref="GameObject"/> reference.</param>
        /// <returns>Always <c>true</c> since the asset is already loaded.</returns>
        protected override bool Validate(out GameObject obj) {
            return obj = GameObject;
        }
    }
}
