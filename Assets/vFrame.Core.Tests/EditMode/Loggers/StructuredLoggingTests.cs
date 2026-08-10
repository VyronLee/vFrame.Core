// ------------------------------------------------------------
//         File: StructuredLoggingTests.cs
//        Brief: Tests for structured logging API with message
//               template and args separation.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-14
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;
using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.Loggers
{
    [TestFixture]
    public class StructuredLoggingTests
    {
        private readonly List<Logger.ILogSink> _registeredSinks = new List<Logger.ILogSink>();
        private readonly List<Logger.IStructuredLogSink> _structuredSinks =
            new List<Logger.IStructuredLogSink>();

        [SetUp]
        public void SetUp() {
            Logger.Close();
            Logger.LogLevel = LogLevelDef.Debug;
            Logger.ApplyConfiguration(new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = "{message}"
            });
            Logger.CaptureStackTrace = false;
        }

        [TearDown]
        public void TearDown() {
            foreach (var sink in _registeredSinks) {
                Logger.RemoveSink(sink);
            }

            foreach (var sink in _structuredSinks) {
                Logger.RemoveStructuredSink(sink);
            }

            _registeredSinks.Clear();
            _structuredSinks.Clear();
            Logger.Close();
        }

        [Test]
        public void StructuredLog_WithArgs_PopulatesArgsField() {
            var sink = RegisterSink();

            Logger.Info(new LogTag("Test"), "User {0} logged in from {1}",
                new object[] { "alice", "192.168.1.1" });

            Assert.That(sink.Entries.Count, Is.EqualTo(1));
            Assert.That(sink.Entries[0].Args, Is.Not.Null);
            Assert.That(sink.Entries[0].Args.Length, Is.EqualTo(2));
            Assert.That(sink.Entries[0].Args[0], Is.EqualTo("alice"));
            Assert.That(sink.Entries[0].Args[1], Is.EqualTo("192.168.1.1"));
        }

        [Test]
        public void StructuredLog_ContentContainsFormattedMessage() {
            var sink = RegisterSink();

            Logger.Info(new LogTag("Test"), "Count: {0}, Name: {1}",
                new object[] { 42, "test" });

            Assert.That(sink.Entries[0].Content, Does.Contain("Count: 42"));
            Assert.That(sink.Entries[0].Content, Does.Contain("Name: test"));
        }

        [Test]
        public void StructuredLog_MessageTemplatePreserved() {
            var sink = RegisterSink();

            Logger.Info(new LogTag("Test"), "User {0} performed {1}",
                new object[] { "bob", "login" });

            Assert.That(sink.Entries[0].MessageTemplate, Is.EqualTo("User {0} performed {1}"));
        }

        [Test]
        public void StructuredLog_WithoutArgs_NullArgs() {
            var sink = RegisterSink();

            Logger.Info(new LogTag("Test"), "simple message", (object[])null);

            Assert.That(sink.Entries[0].Args, Is.Null);
            Assert.That(sink.Entries[0].Content, Does.Contain("simple message"));
        }

        [Test]
        public void StructuredLog_AllLevelsWork() {
            var sink = RegisterSink();
            Logger.LogLevel = LogLevelDef.Trace;
            var tag = new LogTag("Level");

            Logger.Trace(tag, "trace {0}", new object[] { 1 });
            Logger.Debug(tag, "debug {0}", new object[] { 2 });
            Logger.Info(tag, "info {0}", new object[] { 3 });
            Logger.Warning(tag, "warn {0}", new object[] { 4 });
            Logger.Error(tag, "error {0}", new object[] { 5 });
            Logger.Fatal(tag, "fatal {0}", new object[] { 6 });

            Assert.That(sink.Entries.Count, Is.EqualTo(6));
            Assert.That(sink.Entries[0].Level, Is.EqualTo(LogLevelDef.Trace));
            Assert.That(sink.Entries[5].Level, Is.EqualTo(LogLevelDef.Fatal));

            foreach (var entry in sink.Entries) {
                Assert.That(entry.Args, Is.Not.Null);
                Assert.That(entry.Args.Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void JsonLogSink_WithArgs_OutputsArgsArray() {
            var jsonSink = new JsonLogSink();
            Logger.AddStructuredSink(jsonSink);
            _structuredSinks.Add(jsonSink);

            Logger.Info(new LogTag("Json"), "value={0}", new object[] { 42 });

            var output = jsonSink.GetOutput();
            Assert.That(output.Count, Is.EqualTo(1));
            Assert.That(output[0], Does.Contain("\"args\":[42]"));
        }

        [Test]
        public void JsonLogSink_WithoutArgs_OutputsNull() {
            var jsonSink = new JsonLogSink();
            Logger.AddStructuredSink(jsonSink);
            _structuredSinks.Add(jsonSink);

            Logger.Info("plain message");

            var output = jsonSink.GetOutput();
            Assert.That(output.Count, Is.EqualTo(1));
            Assert.That(output[0], Does.Contain("\"args\":null"));
        }

        [Test]
        public void JsonLogSink_PreservesEmptyTagSentinelForUntaggedLogs() {
            var jsonSink = new JsonLogSink();
            Logger.AddStructuredSink(jsonSink);
            _structuredSinks.Add(jsonSink);

            Logger.Info("plain message");

            var output = jsonSink.GetOutput();
            Assert.That(output.Count, Is.EqualTo(1));
            Assert.That(output[0], Does.Contain("\"tag\":\"__EMPTY__\""));
        }

        [Test]
        public void StructuredLog_StackTraceCapturedAtAllLevels_WhenCaptureStackTraceEnabled() {
            Logger.LogLevel = LogLevelDef.Trace;
            Logger.CaptureStackTrace = true;
            var sink = RegisterSink();
            var tag = new LogTag("Stack");

            Logger.Trace(tag, "trace {0}", new object[] { 1 });
            Logger.Debug(tag, "debug {0}", new object[] { 2 });
            Logger.Info(tag, "info {0}", new object[] { 3 });
            Logger.Warning(tag, "warn {0}", new object[] { 4 });
            Logger.Error(tag, "error {0}", new object[] { 5 });
            Logger.Fatal(tag, "fatal {0}", new object[] { 6 });

            Assert.That(sink.Entries.Count, Is.EqualTo(6));
            foreach (var entry in sink.Entries) {
                Assert.That(entry.StackTrace, Is.Not.Null.And.Not.Empty, entry.Level.ToString());
            }
        }

        private RecordingSink RegisterSink(LogLevelDef minLevel = LogLevelDef.Trace) {
            var sink = new RecordingSink();
            Logger.AddSink(sink, minLevel);
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
