// ------------------------------------------------------------
//         File: ArrayPool.cs
//        Brief: High-performance array pooling compatible with System.Buffers.ArrayPool<T> pattern
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-12
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace vFrame.Core
{
    /// <summary>
    /// A thread-safe, bucketized array pool that minimizes GC pressure for large buffer allocations.
    /// Compatible with the <c>System.Buffers.ArrayPool&lt;T&gt;</c> rental pattern:
    /// rent a buffer, use it, then return it to the pool.
    /// </summary>
    /// <typeparam name="T">The element type of the arrays to pool.</typeparam>
    public class VFrameArrayPool<T>
    {
        /// <summary>
        /// Default maximum array length. Arrays requesting more than this are not pooled.
        /// </summary>
        public const int DefaultMaxArrayLength = 1024 * 1024; // 1 MB

        /// <summary>
        /// Default number of arrays per bucket.
        /// </summary>
        public const int DefaultMaxArraysPerBucket = 50;

        /// <summary>
        /// Shared singleton instance with default configuration.
        /// </summary>
        public static readonly VFrameArrayPool<T> Shared = new VFrameArrayPool<T>();

        private readonly int _maxArrayLength;
        private readonly int _maxArraysPerBucket;
        private readonly Bucket[] _buckets;

        /// <summary>
        /// Creates a pool with default settings.
        /// </summary>
        public VFrameArrayPool() : this(DefaultMaxArrayLength, DefaultMaxArraysPerBucket) { }

        /// <summary>
        /// Creates a pool with custom configuration.
        /// </summary>
        /// <param name="maxArrayLength">Maximum array length to pool. Requests exceeding this allocate fresh.</param>
        /// <param name="maxArraysPerBucket">Maximum number of arrays retained per bucket.</param>
        public VFrameArrayPool(int maxArrayLength, int maxArraysPerBucket) {
            if (maxArrayLength <= 0) {
                throw new ArgumentOutOfRangeException(nameof(maxArrayLength));
            }
            if (maxArraysPerBucket <= 0) {
                throw new ArgumentOutOfRangeException(nameof(maxArraysPerBucket));
            }

            _maxArrayLength = maxArrayLength;
            _maxArraysPerBucket = maxArraysPerBucket;

            var bucketCount = SelectBucketIndex(maxArrayLength) + 1;
            _buckets = new Bucket[bucketCount];
            for (var i = 0; i < bucketCount; i++) {
                _buckets[i] = new Bucket(i, maxArraysPerBucket);
            }
        }

        /// <summary>
        /// Rents an array of at least the specified length from the pool.
        /// </summary>
        /// <param name="minimumLength">The minimum length of the returned array.</param>
        /// <returns>An array of at least <paramref name="minimumLength"/> elements.</returns>
        public T[] Rent(int minimumLength) {
            if (minimumLength < 0) {
                throw new ArgumentOutOfRangeException(nameof(minimumLength));
            }
            if (minimumLength == 0) {
                return Array.Empty<T>();
            }

            var index = SelectBucketIndex(minimumLength);
            if (index < _buckets.Length) {
                var bucket = _buckets[index];
                if (bucket.TryPop(out var array)) {
                    return array;
                }
            }

            return new T[GetBucketSize(index)];
        }

        /// <summary>
        /// Returns an array to the pool. The array is only retained if its length is within pool limits.
        /// </summary>
        /// <param name="array">The array to return. May be <c>null</c> (no-op).</param>
        /// <param name="clearArray">
        /// If <c>true</c>, the array contents are cleared before pooling to prevent information leaks.
        /// </param>
        public void Return(T[] array, bool clearArray = false) {
            if (array == null || array.Length == 0) {
                return;
            }

            var index = SelectBucketIndex(array.Length);
            if (index >= _buckets.Length || array.Length > _maxArrayLength) {
                return; // Too large to pool, let GC collect it
            }

            if (clearArray) {
                Array.Clear(array, 0, array.Length);
            }

            _buckets[index].TryPush(array);
        }

        /// <summary>
        /// Gets the total number of arrays currently pooled across all buckets.
        /// </summary>
        public int GetPooledCount() {
            var total = 0;
            for (var i = 0; i < _buckets.Length; i++) {
                total += _buckets[i].Count;
            }
            return total;
        }

        /// <summary>
        /// Selects the bucket index for a given array length using power-of-two bucketing.
        /// </summary>
        private static int SelectBucketIndex(int bufferSize) {
            // BitManipulation.Log2Ceiling-like logic
            bufferSize--;
            bufferSize |= bufferSize >> 1;
            bufferSize |= bufferSize >> 2;
            bufferSize |= bufferSize >> 4;
            bufferSize |= bufferSize >> 8;
            bufferSize |= bufferSize >> 16;
            bufferSize++;

            // log2 of the next power of two
            var log2 = 0;
            while (bufferSize > 1) {
                bufferSize >>= 1;
                log2++;
            }
            return log2;
        }

        /// <summary>
        /// Gets the actual array size for a given bucket index.
        /// </summary>
        private static int GetBucketSize(int index) {
            return 1 << (index > 30 ? 30 : index);
        }

        /// <summary>
        /// A per-size-bucket holding pooled arrays.
        /// </summary>
        private sealed class Bucket
        {
            private readonly int _maxPerBucket;
            private readonly ConcurrentStack<T[]> _stack;

            public Bucket(int bucketIndex, int maxPerBucket) {
                _maxPerBucket = maxPerBucket;
                _stack = new ConcurrentStack<T[]>();
            }

            /// <summary>
            /// Gets the approximate count of arrays in this bucket.
            /// </summary>
            public int Count => _stack.Count;

            public bool TryPop(out T[] array) {
                if (_stack.TryPop(out array)) {
                    return true;
                }

                array = null;
                return false;
            }

            public void TryPush(T[] array) {
                if (_stack.Count >= _maxPerBucket) {
                    return; // Bucket full, discard
                }
                _stack.Push(array);
            }
        }
    }
}
