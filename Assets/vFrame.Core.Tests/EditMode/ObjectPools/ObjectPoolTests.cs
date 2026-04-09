using NUnit.Framework;
using vFrame.Core;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.ObjectPools
{
    public class ObjectPoolTests
    {
        [Test]
        public void Return_DestroysLifecycleObject_InsteadOfReusingTerminatedInstance() {
            var pool = new ObjectPool<PooledState, PooledStateAllocator>(new ObjectPoolOptions<PooledState> {
                InitialCapacity = 0,
                MaxSize = 4
            });
            pool.Create();

            var item = pool.Get();
            item.Value = 42;
            item.Create();

            pool.Return(item);
            var reused = pool.Get();
            var statistics = pool.GetStatistics();

            Assert.That(reused, Is.Not.SameAs(item));
            Assert.That(item.DestroyCallCount, Is.EqualTo(1));
            Assert.That(reused.DestroyCallCount, Is.EqualTo(0));
            Assert.That(item.ResetCallCount, Is.EqualTo(2));
            Assert.That(item.ResetObservedDestroyedState, Is.True);
            Assert.That(reused.ResetCallCount, Is.EqualTo(0));
            Assert.That(reused.Value, Is.EqualTo(0));
            Assert.That(statistics.TotalDestroyedCount, Is.EqualTo(1));
        }

        [Test]
        public void Return_ReusesNonLifecyclePayload_AfterReset() {
            var pool = new ObjectPool<ReusablePayload>(new ObjectPoolOptions<ReusablePayload> {
                InitialCapacity = 0,
                MaxSize = 4
            });
            pool.Create();

            var item = pool.Get();
            item.Value = 99;

            pool.Return(item);
            var reused = pool.Get();

            Assert.That(reused, Is.SameAs(item));
            Assert.That(reused.Value, Is.EqualTo(0));
        }

        [Test]
        public void Return_ReusesAllocatorPayload_AfterAllocatorAndResetHooksRun() {
            var pool = new ObjectPool<AllocatorReusablePayload, AllocatorReusablePayloadAllocator>(new ObjectPoolOptions<AllocatorReusablePayload> {
                InitialCapacity = 0,
                MaxSize = 4
            });
            pool.Create();

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
        public void Return_RetainsPayload_WhenOverflowPolicyIsRetain() {
            var destroyCount = 0;
            var pool = new ObjectPool<PooledPayload>(new ObjectPoolOptions<PooledPayload> {
                InitialCapacity = 0,
                MaxSize = 1,
                OverflowPolicy = ObjectPoolOverflowPolicy.Retain,
                OnDestroy = _ => destroyCount++
            });
            pool.Create();

            var first = pool.Get();
            var second = pool.Get();

            pool.Return(first);
            pool.Return(second);

            var statistics = pool.GetStatistics();

            Assert.That(statistics.CountInactive, Is.EqualTo(2));
            Assert.That(statistics.TotalDestroyedCount, Is.EqualTo(0));
            Assert.That(destroyCount, Is.EqualTo(0));
        }

        [Test]
        public void Return_AppliesCapacityPolicy_WhenPoolIsFull() {
            var destroyCount = 0;
            var pool = new ObjectPool<PooledPayload>(new ObjectPoolOptions<PooledPayload> {
                InitialCapacity = 0,
                MaxSize = 1,
                OverflowPolicy = ObjectPoolOverflowPolicy.DestroyReturned,
                OnDestroy = _ => destroyCount++
            });
            pool.Create();

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
        public void Return_TracksDuplicateReturns_WithoutGrowingInactiveCount() {
            var pool = new ObjectPool<PooledPayload>(new ObjectPoolOptions<PooledPayload> {
                InitialCapacity = 0,
                MaxSize = 2
            });
            pool.Create();

            var item = pool.Get();

            pool.Return(item);
            pool.Return(item);

            var statistics = pool.GetStatistics();

            Assert.That(statistics.CountInactive, Is.EqualTo(1));
            Assert.That(statistics.TotalDuplicateReturnCount, Is.EqualTo(1));
        }

        [Test]
        public void GetStatistics_TracksCreateGetAndReturnCounts_ForReusablePayloads() {
            var pool = new ObjectPool<ReusablePayload>(new ObjectPoolOptions<ReusablePayload> {
                InitialCapacity = 0,
                MaxSize = 4
            });
            pool.Create();

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
            Assert.That(statistics.TotalDestroyedCount, Is.EqualTo(0));
            Assert.That(statistics.TotalDuplicateReturnCount, Is.EqualTo(0));
        }

        [Test]
        public void GetStatistics_ExposesDiagnosticFlags_ForObservedActivity() {
            var pool = new ObjectPool<ReusablePayload>(new ObjectPoolOptions<ReusablePayload> {
                InitialCapacity = 0,
                MaxSize = 2
            });
            pool.Create();

            var item = pool.Get();
            var active = pool.GetStatistics();

            pool.Return(item);
            var returned = pool.GetStatistics();

            Assert.That(active.HasObservedActivity, Is.True);
            Assert.That(active.HasActiveObjects, Is.True);
            Assert.That(active.HasRetainedObjects, Is.False);
            Assert.That(returned.HasRetainedObjects, Is.True);
        }

        private sealed class PooledPayload
        {
        }

        private sealed class ReusablePayload : IPoolObjectResetable
        {
            public int Value { get; set; }

            public void Reset() {
                Value = 0;
            }
        }

        private sealed class AllocatorReusablePayload : IPoolObjectResetable
        {
            public int AllocatorResetCount { get; set; }

            public int InterfaceResetCount { get; private set; }

            public int Value { get; set; }

            public void Reset() {
                InterfaceResetCount++;
                Value = 0;
            }
        }

        private sealed class PooledState : BaseObject, IPoolObjectResetable
        {
            public int DestroyCallCount { get; private set; }

            public int ResetCallCount { get; private set; }

            public bool ResetObservedDestroyedState { get; private set; }

            public int Value { get; set; }

            public void Reset() {
                ResetCallCount++;
                ResetObservedDestroyedState = Destroyed;
                Value = 0;
            }

            protected override void OnCreate() {
            }

            protected override void OnDestroy() {
                DestroyCallCount++;
            }
        }

        private sealed class PooledStateAllocator : IPoolObjectAllocator<PooledState>
        {
            public PooledState Alloc() {
                return new PooledState();
            }

            public void Reset(PooledState obj) {
                obj.Reset();
            }
        }

        private sealed class AllocatorReusablePayloadAllocator : IPoolObjectAllocator<AllocatorReusablePayload>
        {
            public AllocatorReusablePayload Alloc() {
                return new AllocatorReusablePayload();
            }

            public void Reset(AllocatorReusablePayload obj) {
                obj.AllocatorResetCount++;
                obj.Value = 0;
            }
        }
    }
}
