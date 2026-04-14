using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.ObjectPools
{
    public class ObjectPoolTests
    {
        [Test]
        public void Get_ReturnsNonNullInstance() {
            var pool = new ObjectPool<PooledPayload>();
            var item = pool.Get();
            Assert.That(item, Is.Not.Null);
        }

        [Test]
        public void Return_ThenGet_ReusesSameInstance() {
            var pool = new ObjectPool<ReusablePayload>();
            var item = pool.Get();
            pool.Return(item);
            var reused = pool.Get();
            Assert.That(reused, Is.SameAs(item));
        }

        [Test]
        public void Return_ResetsIPoolObjectResetable() {
            var pool = new ObjectPool<ReusablePayload>();
            var item = pool.Get();
            item.Value = 99;
            pool.Return(item);
            var reused = pool.Get();
            Assert.That(reused.Value, Is.EqualTo(0));
        }

        [Test]
        public void Return_ResetsViaAllocatorPolicy() {
            var pool = new ObjectPool<AllocatorReusablePayload>(
                new AllocatorPooledObjectPolicy<AllocatorReusablePayload, AllocatorReusablePayloadAllocator>());
            var item = pool.Get();
            item.Value = 99;
            pool.Return(item);
            var reused = pool.Get();
            Assert.That(reused, Is.SameAs(item));
            Assert.That(reused.Value, Is.EqualTo(0));
            Assert.That(reused.AllocatorResetCount, Is.EqualTo(1));
            Assert.That(reused.InterfaceResetCount, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_FuncFactory_CreatesViaDelegate() {
            var created = 0;
            var pool = new ObjectPool<PooledPayload>(() => {
                created++;
                return new PooledPayload();
            });
            pool.Get();
            pool.Get();
            Assert.That(created, Is.EqualTo(2));
        }

        [Test]
        public void Return_DestroysLifecycleObject_InsteadOfReusingTerminatedInstance() {
            var pool = new ObjectPool<PooledState>(
                new AllocatorPooledObjectPolicy<PooledState, PooledStateAllocator>(),
                new ObjectPoolOptions<PooledState> { MaxSize = 4 });
            var item = pool.Get();
            item.Value = 42;
            item.Create();
            pool.Return(item);
            var reused = pool.Get();
            var statistics = pool.GetStatistics();
            Assert.That(reused, Is.Not.SameAs(item));
            Assert.That(item.DestroyCallCount, Is.EqualTo(1));
            Assert.That(statistics.TotalDestroyedCount, Is.EqualTo(1));
        }

        [Test]
        public void Return_RetainsPayload_WhenOverflowPolicyIsRetain() {
            var destroyCount = 0;
            var pool = new ObjectPool<PooledPayload>(new ObjectPoolOptions<PooledPayload> {
                MaxSize = 1,
                OverflowPolicy = ObjectPoolOverflowPolicy.Retain,
                OnDestroy = _ => destroyCount++
            });
            var first = pool.Get();
            var second = pool.Get();
            pool.Return(first);
            pool.Return(second);
            var statistics = pool.GetStatistics();
            Assert.That(statistics.CountInactive, Is.EqualTo(2));
            Assert.That(destroyCount, Is.EqualTo(0));
        }

        [Test]
        public void Return_AppliesCapacityPolicy_WhenPoolIsFull() {
            var destroyCount = 0;
            var pool = new ObjectPool<PooledPayload>(new ObjectPoolOptions<PooledPayload> {
                MaxSize = 1,
                OverflowPolicy = ObjectPoolOverflowPolicy.DestroyReturned,
                OnDestroy = _ => destroyCount++
            });
            var first = pool.Get();
            var second = pool.Get();
            pool.Return(first);
            pool.Return(second);
            var statistics = pool.GetStatistics();
            Assert.That(statistics.CountInactive, Is.EqualTo(1));
            Assert.That(statistics.TotalDestroyedCount, Is.EqualTo(1));
            Assert.That(destroyCount, Is.EqualTo(1));
        }

        [Test]
        public void Return_TracksDuplicateReturns_WhenCollectionCheckEnabled() {
            var pool = new ObjectPool<PooledPayload>(new ObjectPoolOptions<PooledPayload> {
                MaxSize = 2,
                CollectionCheckEnabled = true
            });
            var item = pool.Get();
            pool.Return(item);
            pool.Return(item);
            var statistics = pool.GetStatistics();
            Assert.That(statistics.CountInactive, Is.EqualTo(1));
            Assert.That(statistics.TotalDuplicateReturnCount, Is.EqualTo(1));
        }

        [Test]
        public void GetStatistics_TracksCreateGetAndReturnCounts() {
            var pool = new ObjectPool<ReusablePayload>(new ObjectPoolOptions<ReusablePayload> { MaxSize = 4 });
            var first = pool.Get();
            var second = pool.Get();
            pool.Return(first);
            pool.Return(second);
            var statistics = pool.GetStatistics();
            Assert.That(statistics.CountAll, Is.EqualTo(2));
            Assert.That(statistics.CountActive, Is.EqualTo(0));
            Assert.That(statistics.CountInactive, Is.EqualTo(2));
            Assert.That(statistics.TotalCreatedCount, Is.EqualTo(2));
            Assert.That(statistics.TotalGetCount, Is.EqualTo(2));
            Assert.That(statistics.TotalReturnCount, Is.EqualTo(2));
        }

        [Test]
        public void Prewarm_CreatesSpecifiedNumberOfInstances() {
            var pool = new ObjectPool<PooledPayload>();
            pool.Prewarm(5);
            var stats = pool.GetStatistics();
            Assert.That(stats.CountAll, Is.EqualTo(5));
            Assert.That(stats.CountInactive, Is.EqualTo(5));
        }

        [Test]
        public void Clear_RemovesAllInactiveObjects() {
            var pool = new ObjectPool<PooledPayload>();
            var item = pool.Get();
            pool.Return(item);
            Assert.That(pool.GetStatistics().CountInactive, Is.EqualTo(1));
            pool.Clear();
            Assert.That(pool.GetStatistics().CountInactive, Is.EqualTo(0));
        }

        [Test]
        public void Get_OutVariant_AutoReturnsOnDispose() {
            var pool = new ObjectPool<PooledPayload>();
            using (pool.Get(out var item)) {
                Assert.That(item, Is.Not.Null);
            }
            var stats = pool.GetStatistics();
            Assert.That(stats.TotalReturnCount, Is.EqualTo(1));
            Assert.That(stats.CountInactive, Is.EqualTo(1));
        }

        // Helper types
        private sealed class PooledPayload { }
        private sealed class ReusablePayload : IPoolObjectResetable {
            public int Value { get; set; }
            public void Reset() { Value = 0; }
        }
        private sealed class AllocatorReusablePayload : IPoolObjectResetable {
            public int AllocatorResetCount { get; set; }
            public int InterfaceResetCount { get; private set; }
            public int Value { get; set; }
            public void Reset() { InterfaceResetCount++; Value = 0; }
        }
        private sealed class AllocatorReusablePayloadAllocator : IPoolObjectAllocator<AllocatorReusablePayload> {
            public AllocatorReusablePayload Alloc() => new AllocatorReusablePayload();
            public void Reset(AllocatorReusablePayload obj) { obj.AllocatorResetCount++; obj.Value = 0; }
        }
        private sealed class PooledState : BaseObject, IPoolObjectResetable {
            public int DestroyCallCount { get; private set; }
            public int Value { get; set; }
            public void Reset() { Value = 0; }
            protected override void OnCreate() { }
            protected override void OnDestroy() { DestroyCallCount++; }
        }
        private sealed class PooledStateAllocator : IPoolObjectAllocator<PooledState> {
            public PooledState Alloc() => new PooledState();
            public void Reset(PooledState obj) => obj.Reset();
        }
    }
}
