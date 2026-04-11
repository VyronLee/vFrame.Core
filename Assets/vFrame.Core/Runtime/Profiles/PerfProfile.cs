// ------------------------------------------------------------
//         File: PerfProfile.cs
//        Brief: Lightweight performance profiling utility for
//                 measuring elapsed time between pin and unpin calls.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2016-07-29 11:01:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Concurrent;
using System.Diagnostics;
using vFrame.Core;

namespace vFrame.Core
{
    public static class PerfProfile
    {
        private static readonly ConcurrentDictionary<int, ProfileData> ProfileDataDict
            = new ConcurrentDictionary<int, ProfileData>();

        private static readonly object LockObject = new object();
        private static readonly LogTag LogTag = new LogTag("PerfProfile");

        private static int _index;

        /// <summary>
        /// Allocates a unique profiling identifier.
        /// </summary>
        /// <param name="id">The allocated profiling identifier.</param>
        public static void Start(out int id) {
            lock (LockObject) {
                id = ++_index;
            }
        }

        /// <summary>
        /// Begins a timed profiling section for the specified identifier.
        /// Only executes when the <c>PERF_PROFILE</c> conditional symbol is defined.
        /// </summary>
        /// <param name="tag">A descriptive label for the profiling section.</param>
        /// <param name="id">The profiling identifier previously allocated by <see cref="Start"/>.</param>
        [Conditional("PERF_PROFILE")]
        public static void Pin(string tag, int id) {
            var data = new ProfileData { Tag = tag };
            data.Stopwatch.Start();

            ProfileDataDict[id] = data;
        }

        /// <summary>
        /// Ends the timed profiling section for the specified identifier and logs the elapsed time.
        /// Only executes when the <c>PERF_PROFILE</c> conditional symbol is defined.
        /// </summary>
        /// <param name="id">The profiling identifier previously allocated by <see cref="Start"/>.</param>
        [Conditional("PERF_PROFILE")]
        public static void Unpin(int id) {
            if (!ProfileDataDict.TryRemove(id, out var data)) {
                return;
            }

            data.Stopwatch.Stop();

            Logger.Info(LogTag, $"{data.Tag}: {data.Stopwatch.Elapsed.TotalMilliseconds:n}ms");
        }

        private class ProfileData
        {
            public Stopwatch Stopwatch { get; } = new Stopwatch();
            public string Tag { get; set; }
        }
    }
}
