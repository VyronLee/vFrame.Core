using UnityEngine;
using UnityEngine.Scripting;
using vFrame.Core.Unity;

namespace vFrame.Core.Benchmarks.Editor
{
    [Preserve]
    [UnityEditor.InitializeOnLoad]
    public static class SpawnPoolsBenchmarks
    {
        private const int SpawnIterations = 20000;
        private const int AsyncIterations = 5000;

        static SpawnPoolsBenchmarks() {
            CoreBenchmarkRunner.Register(RunSpawnRecycleBenchmark);
            CoreBenchmarkRunner.Register(RunSpawnAsyncBenchmark);
        }

        private static CoreBenchmarkRunner.BenchmarkResult RunSpawnRecycleBenchmark() {
            var pools = CreateSpawnPools();

            try {
                return CoreBenchmarkRunner.Measure(
                    "spawnpools.spawn-recycle",
                    64,
                    SpawnIterations,
                    () => {
                        var instance = pools.Spawn("benchmark/sync");
                        pools.Recycle(instance);
                    });
            }
            finally {
                pools.Destroy();
            }
        }

        private static CoreBenchmarkRunner.BenchmarkResult RunSpawnAsyncBenchmark() {
            var pools = CreateSpawnPools();

            try {
                return CoreBenchmarkRunner.Measure(
                    "spawnpools.spawn-async-complete",
                    16,
                    AsyncIterations,
                    () => {
                        var request = pools.SpawnAsync("benchmark/async");
                        var guard = 8;

                        while (!request.IsDone && !request.IsError && guard-- > 0) {
                            pools.Update();
                        }

                        if (request.IsDone && request.GameObject) {
                            pools.Recycle(request.GameObject);
                        }

                        request.Destroy();
                    });
            }
            finally {
                pools.Destroy();
            }
        }

        private static SpawnPools CreateSpawnPools() {
            var pools = new SpawnPools();
            pools.Create(new BenchmarkGameObjectLoaderFactory(), new SpawnPoolsSettings {
                Capacity = 8,
                GCInterval = int.MaxValue,
                LifeTime = int.MaxValue
            });
            return pools;
        }

        private sealed class BenchmarkGameObjectLoaderFactory : IGameObjectLoaderFactory
        {
            public IGameObjectLoader CreateLoader(string assetPath) {
                return new BenchmarkGameObjectLoader(assetPath);
            }
        }

        private sealed class BenchmarkGameObjectLoader : IGameObjectLoader
        {
            private readonly string _assetPath;

            public BenchmarkGameObjectLoader(string assetPath) {
                _assetPath = assetPath;
            }

            public GameObject Load() {
                return new GameObject($"Benchmark({_assetPath})");
            }

            public LoadAsyncRequest LoadAsync() {
                var request = new BenchmarkLoadAsyncRequest {
                    AssetPath = _assetPath,
                    RemainingUpdates = 1
                };

                return request;
            }
        }

        private sealed class BenchmarkLoadAsyncRequest : LoadAsyncRequest
        {
            internal string AssetPath { get; set; }

            internal int RemainingUpdates { get; set; }

            public override float Progress => IsDone ? 1f : 0f;

            protected override void OnDestroy() {
                AssetPath = null;
                RemainingUpdates = 0;
                base.OnDestroy();
            }

            protected override bool Validate(out GameObject obj) {
                if (RemainingUpdates-- > 0) {
                    obj = null;
                    return false;
                }

                obj = new GameObject($"BenchmarkAsync({AssetPath})");
                return true;
            }
        }
    }
}
