using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using vFrame.Core.Unity;

namespace vFrame.Core.Tests.PlayMode.SpawnPools
{
    public class SpawnPoolsValidationTests
    {
        [UnityTest]
        public IEnumerator Spawn_Recycle_AndRespawn_ReusesPoolIdentity() {
            var pools = CreateSpawnPools();
            GameObject respawned = null;

            try {
                var first = pools.Spawn("tests/sync");
                var firstIdentity = first.GetComponent<PoolObjectIdentity>();

                Assert.That(firstIdentity, Is.Not.Null);
                Assert.That(firstIdentity.AssetPath, Is.EqualTo("tests/sync"));
                Assert.That(firstIdentity.IsPooling, Is.False);

                pools.Recycle(first);

                Assert.That(firstIdentity.IsPooling, Is.True);

                respawned = pools.Spawn("tests/sync");
                var respawnedIdentity = respawned.GetComponent<PoolObjectIdentity>();

                Assert.That(respawned, Is.SameAs(first));
                Assert.That(respawnedIdentity, Is.SameAs(firstIdentity));
                Assert.That(respawnedIdentity.UniqueId, Is.EqualTo(firstIdentity.UniqueId));
                Assert.That(respawnedIdentity.IsPooling, Is.False);
            }
            finally {
                if (respawned) {
                    pools.Recycle(respawned);
                }

                pools.Destroy();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator SpawnAsync_Completes_ThroughUpdateLoop() {
            var pools = CreateSpawnPools();
            ILoadAsyncRequest request = null;

            try {
                request = pools.SpawnAsync("tests/async");

                yield return WaitForRequest(pools, request);

                Assert.That(request.IsError, Is.False);
                Assert.That(request.IsDone, Is.True);
                Assert.That(request.GameObject, Is.Not.Null);

                var identity = request.GameObject.GetComponent<PoolObjectIdentity>();

                Assert.That(identity, Is.Not.Null);
                Assert.That(identity.AssetPath, Is.EqualTo("tests/async"));
                Assert.That(identity.IsPooling, Is.False);

                pools.Recycle(request.GameObject);
                request.Destroy();
            }
            finally {
                if (request is { Destroyed: false }) {
                    if (request.IsDone && request.GameObject) {
                        pools.Recycle(request.GameObject);
                    }

                    request.Destroy();
                }

                pools.Destroy();
            }
        }

        [UnityTest]
        public IEnumerator PreloadAsync_Completes_ForMultipleAssetPaths() {
            var pools = CreateSpawnPools();
            IPreloadAsyncRequest request = null;

            try {
                request = pools.PreloadAsync(new[] { "tests/preload/a", "tests/preload/b" });

                yield return WaitForRequest(pools, request);

                Assert.That(request.IsError, Is.False);
                Assert.That(request.IsDone, Is.True);
                Assert.That(request.Progress, Is.EqualTo(1f));

                request.Destroy();
            }
            finally {
                if (request is { Destroyed: false }) {
                    request.Destroy();
                }

                pools.Destroy();
            }
        }

        [UnityTest]
        public IEnumerator Update_ClearsTimedOutInactivePools_ButPreservesActiveReuseFlow() {
            var pools = CreateSpawnPools(new SpawnPoolsSettings {
                Capacity = 8,
                GCInterval = 1,
                LifeTime = -1
            });
            GameObject second = null;

            try {
                var first = pools.Spawn("tests/gc");
                var firstIdentity = first.GetComponent<PoolObjectIdentity>();

                pools.Recycle(first);
                pools.Update();

                second = pools.Spawn("tests/gc");
                var secondIdentity = second.GetComponent<PoolObjectIdentity>();

                Assert.That(second, Is.Not.SameAs(first));
                Assert.That(secondIdentity, Is.Not.Null);
                Assert.That(secondIdentity.AssetPath, Is.EqualTo("tests/gc"));
                Assert.That(secondIdentity.UniqueId, Is.Not.EqualTo(firstIdentity.UniqueId));
                Assert.That(secondIdentity.IsPooling, Is.False);
            }
            finally {
                if (second) {
                    pools.Recycle(second);
                }

                pools.Destroy();
            }

            yield return null;
        }

        private static IEnumerator WaitForRequest(SpawnPools pools, IAsyncRequest request) {
            var guard = 20;

            while (!request.IsDone && !request.IsError && guard-- > 0) {
                pools.Update();
                yield return null;
            }

            Assert.That(guard, Is.GreaterThan(0), "Async request did not complete within the expected update window.");
        }

        private static SpawnPools CreateSpawnPools(SpawnPoolsSettings settings = null) {
            var pools = new SpawnPools();
            pools.Create(new TestGameObjectLoaderFactory(), settings ?? new SpawnPoolsSettings {
                Capacity = 8,
                GCInterval = int.MaxValue,
                LifeTime = int.MaxValue
            });
            return pools;
        }

        private sealed class TestGameObjectLoaderFactory : IGameObjectLoaderFactory
        {
            public IGameObjectLoader CreateLoader(string assetPath) {
                return new TestGameObjectLoader(assetPath);
            }
        }

        private sealed class TestGameObjectLoader : IGameObjectLoader
        {
            private readonly string _assetPath;

            public TestGameObjectLoader(string assetPath) {
                _assetPath = assetPath;
            }

            public GameObject Load() {
                return new GameObject($"SpawnPoolsTest({_assetPath})");
            }

            public LoadAsyncRequest LoadAsync() {
                return new TestLoadAsyncRequest {
                    AssetPath = _assetPath,
                    RemainingUpdates = 1
                };
            }
        }

        private sealed class TestLoadAsyncRequest : LoadAsyncRequest
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

                obj = new GameObject($"SpawnPoolsAsync({_assetPath})");
                return true;
            }
        }
    }
}
