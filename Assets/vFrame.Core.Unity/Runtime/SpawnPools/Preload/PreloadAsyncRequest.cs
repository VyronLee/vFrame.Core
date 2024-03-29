using System.Collections.Generic;
using System.Diagnostics;
using vFrame.Core.Loggers;
using vFrame.Core.Unity.Asynchronous;

namespace vFrame.Core.Unity.SpawnPools
{
    public class PreloadAsyncRequest : AsyncRequest, IPreloadAsyncRequest
    {
        private List<ILoadAsyncRequest> _requests;
        private Stopwatch _stopWatch;
        private int _total;
        internal List<string> AssetPaths { get; set; }
        internal ISpawnPools SpawnPools { get; set; }

        public override float Progress => IsDone ? 1f : (AssetPaths?.Count ?? 0f) / _total;

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

        protected override void OnStop() {
            _requests.Clear();
            _stopWatch.Reset();
        }

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
                    Logger.Error("[SpawnPools] Preload asset failed: {1}, cost: {0:0.000}s", elapsed, path);
                    continue;
                }

                SpawnPools.Recycle(request.GameObject);
                Logger.Info("[SpawnPools] Preload asset finished: {1}, cost: {0:0.000}s", elapsed, path);
            }
        }
    }
}