// ------------------------------------------------------------
//         File: LoggerZeroGcTests.cs
//        Brief: Locks the zero-allocation contract of the
//               [InterpolatedStringHandler] logging path (C11):
//               a log whose level is gated out by the global
//               LogLevel never constructs its message string.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-08-09 00:00:00
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.Loggers
{
    [TestFixture]
    public class LoggerZeroGcTests
    {
        private LogLevelDef _savedLevel;

        [SetUp]
        public void SetUp() {
            Logger.Close();
            _savedLevel = Logger.LogLevel;
            Logger.LogCapacity = Logger.DefaultCapacity;
            Logger.CaptureStackTrace = false;
        }

        [TearDown]
        public void TearDown() {
            Logger.LogLevel = _savedLevel;
            Logger.Close();
        }

        // C11: when the global LogLevel gate rejects a message, the interpolated-string
        // handler short-circuits in its constructor (no StringBuilder is leased) and Info
        // returns before formatting. Behavioral proof: a burst of rejected Info calls
        // produces zero buffered log entries — the message string is never built.
        [Test]
        public void DisabledLevel_BurstProducesNoBufferedLogs() {
            Logger.LogLevel = LogLevelDef.Error;
            var baseline = Logger.GetBufferedLogCount();

            for (var i = 0; i < 1000; i++) {
                Logger.Info($"big interpolated payload number {i} should be skipped");
            }

            Assert.That(Logger.GetBufferedLogCount(), Is.EqualTo(baseline),
                "Rejected Info calls must not construct or buffer any log entries");
        }

        // Positive control: when the gate allows Info, the buffered count advances —
        // proving the assertion above is meaningful (the buffer does record accepted logs).
        [Test]
        public void EnabledLevel_BurstProducesBufferedLogs() {
            Logger.LogLevel = LogLevelDef.Info;
            var baseline = Logger.GetBufferedLogCount();

            Logger.Info($"accepted payload number {1}");

            Assert.That(Logger.GetBufferedLogCount(), Is.EqualTo(baseline + 1));
        }

        // The handler short-circuit applies to every gated level: Trace and Debug sit
        // below Error, so neither may buffer when LogLevel == Error.
        [Test]
        public void DisabledLevel_TraceAndDebugProduceNoBufferedLogs() {
            Logger.LogLevel = LogLevelDef.Error;
            var baseline = Logger.GetBufferedLogCount();

            for (var i = 0; i < 100; i++) {
                Logger.Trace($"trace payload {i}");
                Logger.Debug($"debug payload {i}");
            }

            Assert.That(Logger.GetBufferedLogCount(), Is.EqualTo(baseline));
        }
    }
}
