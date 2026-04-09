using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace vFrame.Core.Benchmarks.Editor
{
    public static class CoreBenchmarkRunner
    {
        public sealed class BenchmarkResult
        {
            public string Name;
            public int Iterations;
            public long ElapsedMilliseconds;

            public double AverageNanosecondsPerOperation {
                get {
                    if (Iterations <= 0) {
                        return 0d;
                    }

                    return ElapsedMilliseconds * 1000000d / Iterations;
                }
            }
        }

        private static readonly List<Func<BenchmarkResult>> _benchmarks = new List<Func<BenchmarkResult>>();

        public static IReadOnlyList<Func<BenchmarkResult>> Benchmarks => _benchmarks;

        public static void Register(Func<BenchmarkResult> benchmark) {
            if (benchmark == null) {
                throw new ArgumentNullException(nameof(benchmark));
            }

            _benchmarks.Add(benchmark);
        }

        public static IReadOnlyList<BenchmarkResult> RunAll() {
            var results = new List<BenchmarkResult>(_benchmarks.Count);

            foreach (var benchmark in _benchmarks) {
                results.Add(benchmark());
            }

            return results;
        }

        [MenuItem("Tools/vFrame/Benchmarks/Run Core Benchmarks")]
        private static void RunAllFromMenu() {
            var results = RunAll();

            foreach (var result in results) {
                UnityEngine.Debug.LogFormat(
                    "[vFrame.Core.Benchmark] {0}: {1} iterations in {2} ms ({3:F2} ns/op)",
                    result.Name,
                    result.Iterations,
                    result.ElapsedMilliseconds,
                    result.AverageNanosecondsPerOperation);
            }
        }

        public static BenchmarkResult Measure(string name, int warmupIterations, int measureIterations, Action action) {
            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentException("Value cannot be null or empty: " + nameof(name));
            }
            if (action == null) {
                throw new ArgumentNullException(nameof(action));
            }
            if (warmupIterations < 0) {
                throw new ArgumentOutOfRangeException(nameof(warmupIterations));
            }
            if (measureIterations <= 0) {
                throw new ArgumentOutOfRangeException(nameof(measureIterations));
            }

            for (var i = 0; i < warmupIterations; i++) {
                action();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < measureIterations; i++) {
                action();
            }
            stopwatch.Stop();

            return new BenchmarkResult {
                Name = name,
                Iterations = measureIterations,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
    }
}