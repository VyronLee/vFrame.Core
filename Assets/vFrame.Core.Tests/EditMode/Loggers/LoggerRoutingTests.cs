using System.Collections.Generic;
using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.Loggers
{
    public class LoggerRoutingTests
    {
        private readonly List<RecordingSink> _registeredSinks = new List<RecordingSink>();

        [SetUp]
        public void SetUp() {
            Logger.Close();
            Logger.LogLevel = LogLevelDef.Debug;
            Logger.ApplyConfiguration(new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = "{message}"
            });
        }

        [TearDown]
        public void TearDown() {
            foreach (var sink in _registeredSinks) {
                Logger.RemoveSink(sink);
            }

            _registeredSinks.Clear();
            Logger.Close();
        }

        [Test]
        public void Log_ReachesAllRegisteredSinks() {
            var first = RegisterSink();
            var second = RegisterSink();

            Logger.Info("fanout");

            Assert.That(first.Entries.Count, Is.EqualTo(1));
            Assert.That(second.Entries.Count, Is.EqualTo(1));
            Assert.That(Logger.GetSinkCount(), Is.EqualTo(2));
        }

        [Test]
        public void RemovingOneSink_DoesNotBreakOtherRegisteredSinks() {
            var first = RegisterSink();
            var second = RegisterSink();

            Logger.RemoveSink(first);
            _registeredSinks.Remove(first);

            Logger.Warning("still-routed");

            Assert.That(first.Entries.Count, Is.EqualTo(0));
            Assert.That(second.Entries.Count, Is.EqualTo(1));
            Assert.That(second.Entries[0].Content, Does.Contain("still-routed"));
        }

        [Test]
        public void LogLevel_Filtering_IsRespectedAcrossMultipleSinks() {
            var first = RegisterSink();
            var second = RegisterSink();
            Logger.LogLevel = LogLevelDef.Warning;

            Logger.Info("skip");
            Logger.Warning("keep");

            Assert.That(first.Entries.Count, Is.EqualTo(1));
            Assert.That(second.Entries.Count, Is.EqualTo(1));
            Assert.That(first.Entries[0].Level, Is.EqualTo(LogLevelDef.Warning));
            Assert.That(second.Entries[0].Level, Is.EqualTo(LogLevelDef.Warning));
        }

        [Test]
        public void BufferedLogCount_IsInspectable() {
            var baseline = Logger.GetBufferedLogCount();

            Logger.Error("inspectable");

            Assert.That(Logger.GetBufferedLogCount(), Is.EqualTo(baseline + 1));
        }

        private RecordingSink RegisterSink() {
            var sink = new RecordingSink();
            Logger.AddSink(sink);
            _registeredSinks.Add(sink);
            return sink;
        }

        private sealed class RecordingSink : Logger.ILogSink
        {
            public List<Logger.LogContext> Entries { get; } = new List<Logger.LogContext>();

            public void OnLogReceived(Logger.LogContext context) {
                Entries.Add(context);
            }
        }
    }
}
