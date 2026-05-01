// ------------------------------------------------------------
//         File: LoggingObjectPool.cs
//        Brief: Decorator that logs all pool operations for diagnostics
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-15 19:09:00
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

namespace vFrame.Core
{
    /// <summary>
    ///     Decorator that logs all pool operations for diagnostics and monitoring.
    /// </summary>
    /// <typeparam name="T">The pooled object type (must be a reference type).</typeparam>
    public sealed class LoggingObjectPool<T> : ObjectPoolDecorator<T> where T : class
    {
        private readonly ILogger _logger;
        private readonly LogTag _logTag;

        /// <summary>
        ///     Creates a new logging decorator wrapping the specified inner pool.
        /// </summary>
        /// <param name="inner">The inner pool to decorate.</param>
        /// <param name="logger">Optional logger instance. If null, uses the static Logger.</param>
        public LoggingObjectPool(IObjectPool<T> inner, ILogger logger = null) : base(inner) {
            _logger = logger;
            _logTag = new LogTag("LoggingObjectPool<" + typeof(T).Name + ">");
        }

        /// <summary>
        ///     Gets an instance from the pool and logs the operation with statistics.
        /// </summary>
        /// <returns>A pooled object instance.</returns>
        public override T Get() {
            var item = base.Get();
            var stats = Inner.GetStatistics();
            Logger.Info(_logTag, "Get() -> {0}, stats: [all={1} active={2} inactive={3}]",
                new object[] {
                    item?.GetHashCode().ToString() ?? "null", stats.CountAll, stats.CountActive, stats.CountInactive
                });
            return item;
        }

        /// <summary>
        ///     Returns an instance to the pool and logs the operation.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        public override void Return(T obj) {
            Logger.Info(_logTag, "Return({0})", new object[] { obj?.GetHashCode().ToString() ?? "null" });
            base.Return(obj);
        }

        /// <summary>
        ///     Removes excess inactive objects from the pool and logs the operation.
        /// </summary>
        /// <param name="maxRetained">Maximum inactive objects to retain.</param>
        /// <returns>The number of objects removed.</returns>
        public override int Trim(int maxRetained) {
            var before = Inner.GetStatistics().CountInactive;
            var removed = base.Trim(maxRetained);
            var after = Inner.GetStatistics().CountInactive;
            Logger.Info(_logTag, "Trim({0}): {1} -> {2}, removed {3}",
                new object[] { maxRetained, before, after, removed });
            return removed;
        }
    }
}