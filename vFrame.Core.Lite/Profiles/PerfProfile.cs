using System.Collections.Concurrent;
using System.Diagnostics;
using vFrame.Core.Loggers;

namespace vFrame.Core.Profiles
{
    public static class PerfProfile
    {
        private class ProfileData
        {
            public Stopwatch Stopwatch { get; } = new Stopwatch();
            public string Tag { get; set; }
        }

        private static readonly ConcurrentDictionary<int, ProfileData> ProfileDataDict
            = new ConcurrentDictionary<int, ProfileData>();

        private static readonly object LockObject = new object();
        private static readonly LogTag LogTag = new LogTag("PerfProfile");

        private static int _index;

        public static void Start(out int id) {
            lock (LockObject) {
                id = ++_index;
            }
        }

        [Conditional("PERF_PROFILE")]
        public static void Pin(string tag, int id) {

            var data = new ProfileData {Tag = tag};
            data.Stopwatch.Start();

            ProfileDataDict[id] = data;
        }

        [Conditional("PERF_PROFILE")]
        public static void Unpin(int id) {
            if (!ProfileDataDict.TryRemove(id, out var data)) {
                return;
            }

            data.Stopwatch.Stop();

            Logger.Info(LogTag, "{0}: {1:n}ms", data.Tag, data.Stopwatch.Elapsed.TotalMilliseconds);
        }
    }
}