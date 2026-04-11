using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.ObjectPools
{
    public class ObjectPoolAdvancedTests
    {
        // --- Phase 1.1: CountAll statistics bug fix ---

        [Test]
        public void CountAll_Decrements_WhenObjectDestroyedOnReturn() {
            var pool = new ObjectPool<PooledState, PooledStateAllocator>(new ObjectPoolOptions<PooledState> {
                InitialCapacity = 0,
                MaxSize = 4
            });
            pool.Create();

            var item = pool.Get();
            item.Create();
            pool.Return(item);

            var stats = pool.GetStatistics();
            Assert.That(stats.CountAll, Is.EqualTo(0), "CountAll should be 0 after the only object was destroyed");
            Assert.That(stats.CountActive, Is.EqualTo(0));
            Assert.That(stats.CountInactive, Is.EqualTo(0));
        }

        [Test]
        public void CountAll_Decrements_WhenOverflowDestroysObject() {
            var pool = new ObjectPool<PooledPayload>(new ObjectPoolOptions<PooledPayload> {
                InitialCapacity = 0,
                MaxSize = 1,
                OverflowPolicy = ObjectPoolOverflowPolicy.DestroyReturned
            });
            pool.Create();

            var first = pool.Get();
            var second = pool.Get();
            pool.Return(first);
            pool.Return(second); // This one should be destroyed due to overflow

            var stats = pool.GetStatistics();
            Assert.That(stats.CountAll, Is.EqualTo(1), "CountAll should be 1 — one retained, one destroyed");
            Assert.That(stats.TotalDestroyedCount, Is.EqualTo(1));
        }

        [Test]
        public void CountActive_NeverNegative_AfterDestroyAndReuse() {
            var pool = new ObjectPool<PooledState, PooledStateAllocator>(new ObjectPoolOptions<PooledState> {
                InitialCapacity = 2,
                MaxSize = 4
            });
            pool.Create();

            var item1 = pool.Get();
            var item2 = pool.Get();
            item1.Create();
            pool.Return(item1); // Destroyed

            var stats = pool.GetStatistics();
            Assert.That(stats.CountActive, Is.GreaterThanOrEqualTo(0), "CountActive should never be negative");
        }

        // --- Phase 1.4: Trim ---

        [Test]
        public void Trim_RemovesExcessInactiveObjects() {
            var pool = new ObjectPool<PooledPayload>(new ObjectPoolOptions<PooledPayload> {
                InitialCapacity = 0,
                MaxSize = 10,
                OverflowPolicy = ObjectPoolOverflowPolicy.Retain
            });
            pool.Create();

            // Get and return 5 items
            var items = new List<PooledPayload>();
            for (var i = 0; i < 5; i++) {
                items.Add(pool.Get());
            }
            foreach (var item in items) {
                pool.Return(item);
            }

            var beforeTrim = pool.GetStatistics();
            Assert.That(beforeTrim.CountInactive, Is.EqualTo(5));

            var removed = pool.Trim(2);
            var afterTrim = pool.GetStatistics();

            Assert.That(removed, Is.EqualTo(3));
            Assert.That(afterTrim.CountInactive, Is.EqualTo(2));
            Assert.That(afterTrim.CountAll, Is.EqualTo(2));
        }

        [Test]
        public void Trim_DoesNotRemoveBelowMaxRetained() {
            var pool = new ObjectPool<PooledPayload>(new ObjectPoolOptions<PooledPayload> {
                InitialCapacity = 0,
                MaxSize = 10,
                OverflowPolicy = ObjectPoolOverflowPolicy.Retain
            });
            pool.Create();

            var items = new List<PooledPayload>();
            for (var i = 0; i < 3; i++) {
                items.Add(pool.Get());
            }
            foreach (var item in items) {
                pool.Return(item);
            }

            var removed = pool.Trim(5);
            Assert.That(removed, Is.EqualTo(0));
            Assert.That(pool.GetStatistics().CountInactive, Is.EqualTo(3));
        }

        [Test]
        public void Trim_WithAllocator_AlsoWorks() {
            var pool = new ObjectPool<ReusablePayload, ReusablePayloadAllocator>(new ObjectPoolOptions<ReusablePayload> {
                InitialCapacity = 0,
                MaxSize = 10,
                OverflowPolicy = ObjectPoolOverflowPolicy.Retain
            });
            pool.Create();

            var items = new List<ReusablePayload>();
            for (var i = 0; i < 4; i++) {
                items.Add(pool.Get());
            }
            foreach (var item in items) {
                pool.Return(item);
            }

            var removed = pool.Trim(1);
            Assert.That(removed, Is.EqualTo(3));
            Assert.That(pool.GetStatistics().CountInactive, Is.EqualTo(1));
        }

        // --- Phase 1.2: ConcurrentObjectPool ---

        [Test]
        public void ConcurrentObjectPool_BasicGetReturn() {
            var pool = new ConcurrentObjectPool<PooledPayload>();
            var item = pool.Get();
            Assert.That(item, Is.Not.Null);
            pool.Return(item);
        }

        [Test]
        public void ConcurrentObjectPool_ResetsOnReturn() {
            var pool = new ConcurrentObjectPool<ReusablePayload>();
            var item = pool.Get();
            item.Value = 42;
            pool.Return(item);

            var reused = pool.Get();
            Assert.That(reused.Value, Is.EqualTo(0), "Should be reset to 0");
        }

        [Test]
        public void ConcurrentObjectPool_ThreadSafety() {
            var pool = new ConcurrentObjectPool<PooledPayload>();
            const int iterations = 10000;
            var threads = new Thread[4];
            var errors = 0;

            for (var t = 0; t < threads.Length; t++) {
                threads[t] = new Thread(() => {
                    try {
                        for (var i = 0; i < iterations; i++) {
                            var item = pool.Get();
                            pool.Return(item);
                        }
                    }
                    catch {
                        Interlocked.Increment(ref errors);
                    }
                });
                threads[t].Start();
            }

            foreach (var thread in threads) {
                thread.Join();
            }

            Assert.That(errors, Is.EqualTo(0), "No exceptions should occur under concurrent access");
        }

        [Test]
        public void ConcurrentObjectPool_WithAllocator_UsesCustomReset() {
            var pool = new ConcurrentObjectPool<ReusablePayload, ReusablePayloadAllocator>();
            var item = pool.Get();
            item.Value = 99;
            pool.Return(item);

            var stats = pool.GetStatistics();
            Assert.That(stats.TotalCreatedCount, Is.EqualTo(1));
            Assert.That(stats.TotalGetCount, Is.EqualTo(1));
            Assert.That(stats.TotalReturnCount, Is.EqualTo(1));
        }

        // --- Phase 1.3: ArrayPool ---

        [Test]
        public void ArrayPool_RentReturnsCorrectSize() {
            var pool = new VFrameArrayPool<byte>();
            var array = pool.Rent(100);
            Assert.That(array.Length, Is.GreaterThanOrEqualTo(100));
            pool.Return(array);
        }

        [Test]
        public void ArrayPool_RentZeroReturnsEmpty() {
            var pool = new VFrameArrayPool<byte>();
            var array = pool.Rent(0);
            Assert.That(array.Length, Is.EqualTo(0));
        }

        [Test]
        public void ArrayPool_ReturnAndReuse() {
            var pool = new VFrameArrayPool<int>();
            var first = pool.Rent(64);
            first[0] = 42;
            pool.Return(first);

            var second = pool.Rent(64);
            // May or may not be the same array, but should be valid
            Assert.That(second.Length, Is.GreaterThanOrEqualTo(64));
            pool.Return(second);
        }

        [Test]
        public void ArrayPool_ClearOnReturn() {
            var pool = new VFrameArrayPool<int>();
            var array = pool.Rent(16);
            for (var i = 0; i < array.Length; i++) {
                array[i] = i;
            }
            pool.Return(array, clearArray: true);

            var reused = pool.Rent(16);
            // If same array, should be cleared
            for (var i = 0; i < reused.Length; i++) {
                Assert.That(reused[i], Is.EqualTo(0), $"Index {i} should be cleared");
            }
            pool.Return(reused);
        }

        [Test]
        public void ArrayPool_OversizedArraysNotPooled() {
            var pool = new VFrameArrayPool<byte>(maxArrayLength: 1024);
            var array = pool.Rent(2048);
            Assert.That(array.Length, Is.GreaterThanOrEqualTo(2048));
            pool.Return(array);

            // The oversized array should not be pooled
            Assert.That(pool.GetPooledCount(), Is.EqualTo(0));
        }

        [Test]
        public void ArrayPool_ThreadSafety() {
            var pool = new VFrameArrayPool<byte>();
            const int iterations = 5000;
            var threads = new Thread[4];
            var errors = 0;

            for (var t = 0; t < threads.Length; t++) {
                threads[t] = new Thread(() => {
                    try {
                        for (var i = 0; i < iterations; i++) {
                            var array = pool.Rent(64);
                            pool.Return(array);
                        }
                    }
                    catch {
                        Interlocked.Increment(ref errors);
                    }
                });
                threads[t].Start();
            }

            foreach (var thread in threads) {
                thread.Join();
            }

            Assert.That(errors, Is.EqualTo(0));
        }

        // --- Phase 1.5: ObjectPoolManager enhancements ---

        [Test]
        public void ObjectPoolManager_TryGetObjectPool_ReturnsFalseWhenNotRegistered() {
            var mgr = new ObjectPoolManager();
            mgr.Create();
            try {
                var result = mgr.TryGetObjectPool<PooledPayload>(out var pool);
                Assert.That(result, Is.False);
                Assert.That(pool, Is.Null);
            }
            finally {
                mgr.Destroy();
            }
        }

        [Test]
        public void ObjectPoolManager_TryGetObjectPool_ReturnsTrueWhenRegistered() {
            var mgr = new ObjectPoolManager();
            mgr.Create();
            try {
                mgr.GetObjectPool<PooledPayload>(); // Register
                var result = mgr.TryGetObjectPool<PooledPayload>(out var pool);
                Assert.That(result, Is.True);
                Assert.That(pool, Is.Not.Null);
            }
            finally {
                mgr.Destroy();
            }
        }

        [Test]
        public void ObjectPoolManager_GetPoolCount_TracksRegistrations() {
            var mgr = new ObjectPoolManager();
            mgr.Create();
            try {
                Assert.That(mgr.GetPoolCount(), Is.EqualTo(0));
                mgr.GetObjectPool<PooledPayload>();
                Assert.That(mgr.GetPoolCount(), Is.EqualTo(1));
                mgr.GetObjectPool<List<int>>();
                Assert.That(mgr.GetPoolCount(), Is.EqualTo(2));
            }
            finally {
                mgr.Destroy();
            }
        }

        [Test]
        public void ObjectPoolManager_Destroy_CleansUpAllPools() {
            var mgr = new ObjectPoolManager();
            mgr.Create();
            try {
                mgr.GetObjectPool<PooledPayload>();
                mgr.GetObjectPool<List<int>>();
                Assert.That(mgr.GetPoolCount(), Is.EqualTo(2));
            }
            finally {
                mgr.Destroy();
            }
            // After destroy, all pools should be cleaned
            Assert.That(mgr.GetPoolCount(), Is.EqualTo(0));
        }

        // --- Builtin collection pool tests ---

        [Test]
        public void ListPool_GetReturn_ResetsList() {
            var pool = ListPool<int>.Shared;
            var list = pool.Get();
            list.Add(1);
            list.Add(2);
            list.Add(3);
            Assert.That(list.Count, Is.EqualTo(3));

            pool.Return(list);
            var reused = pool.Get();
            Assert.That(reused.Count, Is.EqualTo(0), "List should be cleared after return");
        }

        [Test]
        public void DictionaryPool_GetReturn_ResetsDictionary() {
            var pool = DictionaryPool<string, int>.Shared;
            var dict = pool.Get();
            dict["a"] = 1;
            dict["b"] = 2;
            Assert.That(dict.Count, Is.EqualTo(2));

            pool.Return(dict);
            var reused = pool.Get();
            Assert.That(reused.Count, Is.EqualTo(0), "Dictionary should be cleared after return");
        }

        [Test]
        public void HashSetPool_GetReturn_ResetsSet() {
            var pool = HashSetPool<int>.Shared;
            var set = pool.Get();
            set.Add(1);
            set.Add(2);
            Assert.That(set.Count, Is.EqualTo(2));

            pool.Return(set);
            var reused = pool.Get();
            Assert.That(reused.Count, Is.EqualTo(0), "HashSet should be cleared after return");
        }

        [Test]
        public void QueuePool_GetReturn_ResetsQueue() {
            var pool = QueuePool<int>.Shared;
            var queue = pool.Get();
            queue.Enqueue(1);
            queue.Enqueue(2);
            Assert.That(queue.Count, Is.EqualTo(2));

            pool.Return(queue);
            var reused = pool.Get();
            Assert.That(reused.Count, Is.EqualTo(0), "Queue should be cleared after return");
        }

        [Test]
        public void StackPool_GetReturn_ResetsStack() {
            var pool = StackPool<int>.Shared;
            var stack = pool.Get();
            stack.Push(1);
            stack.Push(2);
            Assert.That(stack.Count, Is.EqualTo(2));

            pool.Return(stack);
            var reused = pool.Get();
            Assert.That(reused.Count, Is.EqualTo(0), "Stack should be cleared after return");
        }

        [Test]
        public void StringBuilderPool_GetReturn_ResetsBuilder() {
            var pool = StringBuilderPool.Shared;
            var sb = pool.Get();
            sb.Append("Hello World");
            Assert.That(sb.Length, Is.GreaterThan(0));

            pool.Return(sb);
            var reused = pool.Get();
            Assert.That(reused.Length, Is.EqualTo(0), "StringBuilder should be cleared after return");
        }

        // --- Helper types ---

        private sealed class PooledPayload { }

        private sealed class ReusablePayload : IPoolObjectResetable
        {
            public int Value { get; set; }

            public void Reset() {
                Value = 0;
            }
        }

        private sealed class ReusablePayloadAllocator : IPoolObjectAllocator<ReusablePayload>
        {
            public ReusablePayload Alloc() => new ReusablePayload();

            public void Reset(ReusablePayload obj) {
                obj.Value = 0;
            }
        }

        private sealed class PooledState : BaseObject, IPoolObjectResetable
        {
            public int DestroyCallCount { get; private set; }
            public int Value { get; set; }

            public void Reset() {
                Value = 0;
            }

            protected override void OnCreate() { }

            protected override void OnDestroy() {
                DestroyCallCount++;
            }
        }

        private sealed class PooledStateAllocator : IPoolObjectAllocator<PooledState>
        {
            public PooledState Alloc() => new PooledState();

            public void Reset(PooledState obj) {
                obj.Reset();
            }
        }
    }
}
