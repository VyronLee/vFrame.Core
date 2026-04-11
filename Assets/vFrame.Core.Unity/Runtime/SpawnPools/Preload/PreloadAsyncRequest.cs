// ------------------------------------------------------------
//         File: PreloadAsyncRequest.cs
//        Brief: Async request that preloads multiple assets through the spawn pool system.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;
using System.Diagnostics;
using vFrame.Core;
using vFrame.Core.Unity;

namespace vFrame.Core.Unity
{
    public class PreloadAsyncRequest : AsyncRequest, IPreloadAsyncRequest
    {
        private List<ILoadAsyncRequest> _requests;
        private Stopwatch _stopWatch;
        private int _total;

        internal List<string> AssetPaths { get; set; }

        internal ISpawnPools SpawnPools { get; set; }

        /// <summary>
        /// Gets the overall preload progress as a normalized value between 0 and 1.
        /// </summary>
        public override float Progress => IsDone ? 1f : 1f - (AssetPaths?.Count ?? 0f) / _total;

        /// <summary>
        /// Starts the preload operation by spawning async requests for every asset path.
        /// Finishes immediately when the asset path list is empty or null.
        /// </summary>
        protected override void OnStart() {
            if (null == AssetPaths || AssetPaths.Count <= 0) {
                Finish();
                return;
            }

            _total = AssetPaths.Count;
            _stopWatch = Stopwatch.StartNew();
            _requests = new List<ILoadAsyncRequest>(AssetPaths.Count);
            foreach (var path in AssetPaths) {
                _requests.Add(SpawnPools.SpawnAsync(path));
            }
        }

        /// <summary>
        /// Clears pending requests and resets the stopwatch when the operation is stopped.
        /// </summary>
        protected override void OnStop() {
            _requests.Clear();
            _stopWatch.Reset();
        }

        /// <summary>
        /// Polls pending load requests each frame, removes completed ones,
        /// recycles successfully loaded GameObjects, and finishes when all requests are done.
        /// </summary>
        protected override void OnUpdate() {
            if (_requests.Count <= 0) {
                Finish();
                return;
            }

            for (var index = _requests.Count - 1; index >= 0; index--) {
                var request = _requests[index];
                if (!request.IsDone && !request.IsError) {
                    continue;
                }

                var path = AssetPaths[index];
                AssetPaths.RemoveAt(index);
                _requests.RemoveAt(index);

                var elapsed = _stopWatch.Elapsed.TotalSeconds;
                if (request.IsError) {
                    Logger.Error($"[SpawnPools] Preload asset failed: {path}, cost: {elapsed:0.000}s");
                    continue;
                }

                SpawnPools.Recycle(request.GameObject);
                Logger.Info($"[SpawnPools] Preload asset finished: {path}, cost: {elapsed:0.000}s");
            }
        }
    }
}
