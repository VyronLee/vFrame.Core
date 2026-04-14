using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.ObjectPools
{
    public class ObjectPoolAdvancedTests
    {
        // --- Trim ---
        [Test]
        public void Trim_RemovesExcessInactiveObjects() {
            var pool = new ObjectPool<PooledPayload>(new ObjectPoolOptions<PooledPayload> {
                MaxSize = 10, OverflowPolicy = ObjectPoolOverflowPolicy.Retain
            });
            var items = new List<PooledPayload>();
            for (var i = 0; i < 5; i++) items.Add(pool.Get());
            foreach (var item in items) pool.Return(item);
            Assert.That(pool.GetStatistics().CountInactive, Is.EqualTo(5));
            var removed = pool.Trim(2);
            Assert.That(removed, Is.EqualTo(3));
            Assert.That(pool.GetStatistics().CountInactive, Is.EqualTo(2));
        }

        [Test]
        public void Trim_DoesNotRemoveBelowMaxRetained() {
            var pool = new ObjectPool<PooledPayload>(new ObjectPoolOptions<PooledPayload> {
                MaxSize = 10, OverflowPolicy = ObjectPoolOverflowPolicy.Retain
            });
            var items = new List<PooledPayload>();
            for (var i = 0; i < 3; i++) items.Add(pool.Get());
            foreach (var item in items) pool.Return(item);
            var removed = pool.Trim(5);
            Assert.That(removed, Is.EqualTo(0));
        }

        // --- ConcurrentObjectPool ---
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
            Assert.That(reused.Value, Is.EqualTo(0));
        }

        [Test]
        public void ConcurrentObjectPool_ThreadSafety() {
            var pool = new ConcurrentObjectPool<PooledPayload>();
            const int iterations = 10000;
            var threads = new Thread[4];
            var errors = 0;
            for (var t = 0; t < threads.Length; t++) {
                threads[t] = new Thread(() => {
                    try { for (var i = 0; i < iterations; i++) { var item = pool.Get(); pool.Return(item); } }
                    catch { Interlocked.Increment(ref errors); }
                });
                threads[t].Start();
            }
            foreach (var thread in threads) thread.Join();
            Assert.That(errors, Is.EqualTo(0));
        }

        [Test]
        public void ConcurrentObjectPool_Trim() {
            var pool = new ConcurrentObjectPool<PooledPayload>();
            for (var i = 0; i < 5; i++) pool.Return(pool.Get());
            var removed = pool.Trim(2);
            Assert.That(removed, Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void ConcurrentObjectPool_WithPolicy() {
            var pool = new ConcurrentObjectPool<ReusablePayload>(
                new AllocatorPooledObjectPolicy<ReusablePayload, ReusablePayloadAllocator>());
            var item = pool.Get();
            item.Value = 99;
            pool.Return(item);
            var stats = pool.GetStatistics();
            Assert.That(stats.TotalCreatedCount, Is.EqualTo(1));
        }

        [Test]
        public void ConcurrentObjectPool_AutoReturn() {
            var pool = new ConcurrentObjectPool<PooledPayload>();
            using (pool.Get(out var item)) { Assert.That(item, Is.Not.Null); }
            Assert.That(pool.GetStatistics().TotalReturnCount, Is.EqualTo(1));
        }

        // --- ObjectPoolManager ---
        [Test]
        public void ObjectPoolManager_TryGetObjectPool_ReturnsFalseWhenNotRegistered() {
            var mgr = new ObjectPoolManager();
            mgr.Create();
            try {
                var result = mgr.TryGetObjectPool<PooledPayload>(out var pool);
                Assert.That(result, Is.False);
                Assert.That(pool, Is.Null);
            } finally { mgr.Destroy(); }
        }

        [Test]
        public void ObjectPoolManager_TryGetObjectPool_ReturnsTrueWhenRegistered() {
            var mgr = new ObjectPoolManager();
            mgr.Create();
            try {
                mgr.GetObjectPool<PooledPayload>();
                var result = mgr.TryGetObjectPool<PooledPayload>(out var pool);
                Assert.That(result, Is.True);
                Assert.That(pool, Is.Not.Null);
            } finally { mgr.Destroy(); }
        }

        [Test]
        public void ObjectPoolManager_Register_AndUnregister() {
            var mgr = new ObjectPoolManager();
            mgr.Create();
            try {
                var pool = new ObjectPool<PooledPayload>();
                mgr.Register(pool);
                Assert.That(mgr.GetPoolCount(), Is.EqualTo(1));
                Assert.That(mgr.Unregister<PooledPayload>(), Is.True);
                Assert.That(mgr.GetPoolCount(), Is.EqualTo(0));
                Assert.That(mgr.Unregister<PooledPayload>(), Is.False);
            } finally { mgr.Destroy(); }
        }

        [Test]
        public void ObjectPoolManager_GetAllPools() {
            var mgr = new ObjectPoolManager();
            mgr.Create();
            try {
                mgr.GetObjectPool<PooledPayload>();
                mgr.GetObjectPool<List<int>>();
                var all = new List<(Type, IObjectPool)>(mgr.GetAllPools());
                Assert.That(all.Count, Is.EqualTo(2));
            } finally { mgr.Destroy(); }
        }

        [Test]
        public void ObjectPoolManager_TrimAll_ActuallyTrims() {
            var mgr = new ObjectPoolManager();
            mgr.Create();
            try {
                var pool = mgr.GetObjectPool<PooledPayload>();
                var items = new List<PooledPayload>();
                for (var i = 0; i < 5; i++) items.Add(pool.Get());
                foreach (var item in items) pool.Return(item);
                Assert.That(pool.GetStatistics().CountInactive, Is.EqualTo(5));
                var totalRemoved = mgr.TrimAll(2);
                Assert.That(totalRemoved, Is.EqualTo(3));
                Assert.That(pool.GetStatistics().CountInactive, Is.EqualTo(2));
            } finally { mgr.Destroy(); }
        }

        // --- Builtin collection pools ---
        [Test]
        public void ListPool_GetReturn_ResetsList() {
            var pool = ListPool<int>.Shared;
            var list = pool.Get();
            list.Add(1); list.Add(2); list.Add(3);
            pool.Return(list);
            Assert.That(pool.Get().Count, Is.EqualTo(0));
        }

        [Test]
        public void DictionaryPool_GetReturn_ResetsDictionary() {
            var pool = DictionaryPool<string, int>.Shared;
            var dict = pool.Get();
            dict["a"] = 1; dict["b"] = 2;
            pool.Return(dict);
            Assert.That(pool.Get().Count, Is.EqualTo(0));
        }

        [Test]
        public void HashSetPool_GetReturn_ResetsSet() {
            var pool = HashSetPool<int>.Shared;
            var set = pool.Get();
            set.Add(1); set.Add(2);
            pool.Return(set);
            Assert.That(pool.Get().Count, Is.EqualTo(0));
        }

        [Test]
        public void QueuePool_GetReturn_ResetsQueue() {
            var pool = QueuePool<int>.Shared;
            var queue = pool.Get();
            queue.Enqueue(1); queue.Enqueue(2);
            pool.Return(queue);
            Assert.That(pool.Get().Count, Is.EqualTo(0));
        }

        [Test]
        public void StackPool_GetReturn_ResetsStack() {
            var pool = StackPool<int>.Shared;
            var stack = pool.Get();
            stack.Push(1); stack.Push(2);
            pool.Return(stack);
            Assert.That(pool.Get().Count, Is.EqualTo(0));
        }

        [Test]
        public void StringBuilderPool_GetReturn_ResetsBuilder() {
            var pool = StringBuilderPool.Shared;
            var sb = pool.Get();
            sb.Append("Hello World");
            pool.Return(sb);
            Assert.That(pool.Get().Length, Is.EqualTo(0));
        }

        // --- Decorator tests ---
        [Test]
        public void LeakTrackingObjectPool_TracksAndGetReturns() {
            var inner = new ObjectPool<PooledPayload>();
            var pool = new LeakTrackingObjectPool<PooledPayload>(inner);
            var item = pool.Get();
            Assert.That(pool.GetLeakCount(), Is.GreaterThanOrEqualTo(1));
            pool.Return(item);
            Assert.That(pool.GetLeakCount(), Is.EqualTo(0));
        }

        [Test]
        public void ValidatingObjectPool_DiscardsInvalidOnReturn() {
            var inner = new ObjectPool<PooledPayload>();
            var pool = new ValidatingObjectPool<PooledPayload>(inner, _ => true);
            pool.Return(pool.Get());
            Assert.That(inner.GetStatistics().CountInactive, Is.EqualTo(1));
        }

        [Test]
        public void ValidatingObjectPool_DiscardsInvalid_WhenValidatorFails() {
            var inner = new ObjectPool<PooledPayload>();
            var pool = new ValidatingObjectPool<PooledPayload>(inner, _ => false);
            pool.Return(pool.Get());
            Assert.That(inner.GetStatistics().CountInactive, Is.EqualTo(0));
        }

        // Helper types
        private sealed class PooledPayload { }
        private sealed class ReusablePayload : IPoolObjectResetable {
            public int Value { get; set; }
            public void Reset() { Value = 0; }
        }
        private sealed class ReusablePayloadAllocator : IPoolObjectAllocator<ReusablePayload> {
            public ReusablePayload Alloc() => new ReusablePayload();
            public void Reset(ReusablePayload obj) => obj.Value = 0;
        }
    }
}
