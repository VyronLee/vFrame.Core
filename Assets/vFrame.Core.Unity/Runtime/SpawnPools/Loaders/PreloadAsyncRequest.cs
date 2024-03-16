using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using vFrame.Core.Asynchronous;
using vFrame.Core.Loggers;

namespace vFrame.Core.SpawnPools.Loaders
{
    public class PreloadAsyncRequest : AsyncRequest<SpawnPools, string[]>, IPreloadAsyncRequest
    {
        protected override IEnumerator OnProcess(SpawnPools spawnPools, string[] assetPaths) {
            var paths = new List<string>(assetPaths);
            var requests = new List<ILoaderAsyncRequest>(paths.Count);
            var stopWatch = Stopwatch.StartNew();
            foreach (var path in paths) {
                var req = spawnPools[path].SpawnAsync();
                requests.Add(req);
            }

            while (requests.Count > 0) {
                for (var index = requests.Count - 1; index >= 0; index--) {
                    var request = requests[index];
                    if (request.MoveNext())
                        continue;

                    var path = paths[index];
                    requests.RemoveAt(index);
                    paths.RemoveAt(index);

                    spawnPools[path].Recycle(request.GetGameObject());

                    var elapsed = stopWatch.Elapsed.TotalSeconds;
                    Logger.Info("[SpawnPools] Preload asset finished, cost: {0:0.000}s, path: {1}", elapsed, path);
                }
                yield return null;
            }
            stopWatch.Stop();
        }
    }
}