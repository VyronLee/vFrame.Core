using vFrame.Core.ObjectPools;

namespace vFrame.Core.Benchmarks.Editor
{
    [UnityEditor.InitializeOnLoad]
    public static class ObjectPoolBenchmarks
    {
        private const int PoolIterations = 200000;

        static ObjectPoolBenchmarks() {
            CoreBenchmarkRunner.Register(RunGenericPoolBenchmark);
        }

        private static CoreBenchmarkRunner.BenchmarkResult RunGenericPoolBenchmark() {
            var pool = new ObjectPool<PooledPayload, PooledPayloadAllocator>();
            pool.Create();

            var result = CoreBenchmarkRunner.Measure(
                "pool.generic.get-return",
                512,
                PoolIterations,
                () => {
                    var payload = pool.Get();
                    payload.Sequence++;
                    pool.Return(payload);
                });

            pool.Destroy();
            return result;
        }

        private sealed class PooledPayload : IPoolObjectResetable
        {
            public int Sequence;

            public void Reset() {
                Sequence = 0;
            }
        }

        private sealed class PooledPayloadAllocator : IPoolObjectAllocator<PooledPayload>
        {
            public PooledPayload Alloc() {
                return new PooledPayload();
            }

            public void Reset(PooledPayload obj) {
                obj.Sequence = 0;
            }
        }
    }
}
