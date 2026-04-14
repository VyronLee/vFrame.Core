// ------------------------------------------------------------
//         File: LoggerBaselineTests.cs
//        Brief: Characterization tests capturing current Logger
//               behavior as the baseline for refactoring.
//
//       Author: Hermes Agent
//
//      Created: 2026-04-11
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Generic;
using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.Loggers
{
    [TestFixture]
    public class LoggerBaselineTests
    {
        private readonly List<Logger.ILogSink> _registeredSinks = new List<Logger.ILogSink>();

        [SetUp]
        public void SetUp() {
            Logger.Close();
            Logger.LogLevel = LogLevelDef.Debug;
            Logger.ApplyConfiguration(new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = LogTemplates.Default
            });
            Logger.LogCapacity = Logger.DefaultCapacity;
            Logger.CaptureStackTrace = false;
        }

        [TearDown]
        public void TearDown() {
            foreach (var sink in _registeredSinks) {
                Logger.RemoveSink(sink);
            }

            _registeredSinks.Clear();
            Logger.Close();
        }

        // ── Level Filtering ────────────────────────────────────

        [Test]
        public void LogLevel_WarningBlocksDebugAndInfo() {
            var sink = RegisterSink();
            Logger.LogLevel = LogLevelDef.Warning;

            Logger.Debug("debug-msg");
            Logger.Info("info-msg");
            Logger.Warning("warning-msg");
            Logger.Error("error-msg");
            Logger.Fatal("fatal-msg");

            Assert.That(sink.Entries.Count, Is.EqualTo(3));
            Assert.That(sink.Entries[0].Level, Is.EqualTo(LogLevelDef.Warning));
            Assert.That(sink.Entries[1].Level, Is.EqualTo(LogLevelDef.Error));
            Assert.That(sink.Entries[2].Level, Is.EqualTo(LogLevelDef.Fatal));
        }

        [Test]
        public void LogLevel_ErrorBlocksWarningAndBelow() {
            var sink = RegisterSink();
            Logger.LogLevel = LogLevelDef.Error;

            Logger.Warning("should-be-blocked");
            Logger.Error("should-pass");

            Assert.That(sink.Entries.Count, Is.EqualTo(1));
            Assert.That(sink.Entries[0].Level, Is.EqualTo(LogLevelDef.Error));
        }

        [Test]
        public void LogLevel_DebugAllowsEverything() {
            var sink = RegisterSink();
            Logger.LogLevel = LogLevelDef.Debug;

            Logger.Debug("msg");

            Assert.That(sink.Entries.Count, Is.EqualTo(1));
        }

        // ── Buffer Behavior ────────────────────────────────────

        [Test]
        public void BufferOverflowsRemovesOldest() {
            Logger.LogCapacity = 2;
            Logger.LogLevel = LogLevelDef.Debug;

            Logger.Info("first");
            Logger.Info("second");
            Logger.Info("third");

            Assert.That(Logger.GetBufferedLogCount(), Is.EqualTo(2));

            var logs = new List<Logger.LogContext>(Logger.Logs(-1));
            Assert.That(logs.Count, Is.EqualTo(2));
            Assert.That(logs[0].Content, Does.Contain("second"));
            Assert.That(logs[1].Content, Does.Contain("third"));
        }

        [Test]
        public void BufferedLogCountIncrements() {
            Logger.LogLevel = LogLevelDef.Debug;
            var baseline = Logger.GetBufferedLogCount();

            Logger.Info("one");
            Logger.Info("two");

            Assert.That(Logger.GetBufferedLogCount(), Is.EqualTo(baseline + 2));
        }

        [Test]
        public void LogsFilteredByMask() {
            Logger.LogLevel = LogLevelDef.Debug;
            Logger.Info("info-entry");
            Logger.Error("error-entry");

            var errorLogs = new List<Logger.LogContext>(Logger.Logs((int)LogLevelDef.Error));
            Assert.That(errorLogs.Count, Is.EqualTo(1));
            Assert.That(errorLogs[0].Level, Is.EqualTo(LogLevelDef.Error));
        }

        // ── Format Output ──────────────────────────────────────

        [Test]
        public void DefaultFormatContainsTagAndTimeAndLevel() {
            Logger.LogLevel = LogLevelDef.Debug;
            Logger.ApplyConfiguration(new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = LogTemplates.Default
            });
            var sink = RegisterSink();

            Logger.Info(new LogTag("TestTag"), "hello");

            Assert.That(sink.Entries.Count, Is.EqualTo(1));
            var content = sink.Entries[0].Content;
            Assert.That(content, Does.Contain("TestTag"));
            Assert.That(content, Does.Match(@"\[\d{4}-\d{2}-\d{2}"));
            Assert.That(content, Does.Contain("INF"));
            Assert.That(content, Does.Contain("hello"));
        }

        [Test]
        public void CompactFormatOnlyContainsLevelAndMessage() {
            Logger.LogLevel = LogLevelDef.Debug;
            Logger.ApplyConfiguration(new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = LogTemplates.Compact
            });
            var sink = RegisterSink();

            Logger.Info(new LogTag("OnlyTag"), "msg");

            var content = sink.Entries[0].Content;
            Assert.That(content, Does.Contain("INF"));
            Assert.That(content, Does.Contain("msg"));
            // Compact format does not include time or tag
            Assert.That(content, Does.Not.Match(@"\[\d{4}-\d{2}-\d{2}"));
        }

        [Test]
        public void StringFormatAppliedToArgs() {
            Logger.LogLevel = LogLevelDef.Debug;
            Logger.ApplyConfiguration(new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = "{message}"
            });
            var sink = RegisterSink();

            var name = "test";
            Logger.Info($"count={42}, name={name}");

            var content = sink.Entries[0].Content;
            Assert.That(content, Does.Contain("count=42"));
            Assert.That(content, Does.Contain("name=test"));
        }

        // ── Tag Behavior ───────────────────────────────────────

        [Test]
        public void LogWithTagIncludesTagInOutput() {
            Logger.LogLevel = LogLevelDef.Debug;
            Logger.ApplyConfiguration(new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = LogTemplates.Default
            });
            var sink = RegisterSink();

            Logger.Warning(new LogTag("MyModule"), "issue");

            Assert.That(sink.Entries[0].Tag.Equals(new LogTag("MyModule")));
            Assert.That(sink.Entries[0].Content, Does.Contain("MyModule"));
        }

        [Test]
        public void LogWithoutTagOmitsTagFromOutput() {
            Logger.LogLevel = LogLevelDef.Debug;
            Logger.ApplyConfiguration(new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = LogTemplates.Default
            });
            var sink = RegisterSink();

            Logger.Info("no-tag-msg");

            var content = sink.Entries[0].Content;
            Assert.That(content, Does.Not.Contain("__EMPTY__"));
        }

        // ── LogContext Fields ───────────────────────────────────

        [Test]
        public void LogContextContainsCorrectLevelAndContent() {
            Logger.LogLevel = LogLevelDef.Debug;
            var sink = RegisterSink();

            Logger.Error("err-msg");

            Assert.That(sink.Entries[0].Level, Is.EqualTo(LogLevelDef.Error));
            Assert.That(sink.Entries[0].Content, Does.Contain("err-msg"));
            Assert.That(sink.Entries[0].Exception, Is.Null);
        }

        // ── Sink Routing ───────────────────────────────────────

        [Test]
        public void MultipleSinksAllReceiveLogs() {
            var first = RegisterSink();
            var second = RegisterSink();

            Logger.Info("fanout");

            Assert.That(first.Entries.Count, Is.EqualTo(1));
            Assert.That(second.Entries.Count, Is.EqualTo(1));
        }

        [Test]
        public void RemovedSinkStopsReceiving() {
            var sink = RegisterSink();
            Logger.RemoveSink(sink);
            _registeredSinks.Remove(sink);

            Logger.Info("after-remove");

            Assert.That(sink.Entries.Count, Is.EqualTo(0));
        }

        [Test]
        public void DuplicateSinkRegistrationIsIdempotent() {
            var sink = RegisterSink();
            Logger.AddSink(sink); // register again

            Logger.Info("dup-test");

            Assert.That(sink.Entries.Count, Is.EqualTo(1));
            Assert.That(Logger.GetSinkCount(), Is.EqualTo(1));
        }

        // ── OnLogReceived Event ────────────────────────────────

        [Test]
        public void OnLogReceivedEventFires() {
            Logger.LogLevel = LogLevelDef.Debug;
            Logger.LogContext received = default;
            Logger.OnLogReceived += ctx => received = ctx;

            Logger.Info("event-test");

            Assert.That(received.Level, Is.EqualTo(LogLevelDef.Info));
            Assert.That(received.Content, Does.Contain("event-test"));
        }

        // ── Caller Info Injection ─────────────────────────────

        [Test]
        public void LogContext_ContainsFormattedMessage() {
            Logger.LogLevel = LogLevelDef.Debug;
            var sink = RegisterSink();

            Logger.Info($"value={42}");

            Assert.That(sink.Entries[0].Content, Does.Contain("value=42"));
        }

        [Test]
        public void LogContext_ContainsFormattedText() {
            Logger.LogLevel = LogLevelDef.Debug;
            var sink = RegisterSink();

            Logger.Info("plain message");

            Assert.That(sink.Entries[0].Content, Does.Contain("plain message"));
        }

        [Test]
        public void LogContext_StackTraceNullByDefault() {
            Logger.LogLevel = LogLevelDef.Debug;
            Logger.CaptureStackTrace = false;
            var sink = RegisterSink();

            Logger.Info("no-stack");
            Logger.Error("err-no-stack");

            // CaptureStackTrace=false and no exception → StackTrace is null
            Assert.That(sink.Entries[0].StackTrace, Is.Null);
            // Error without CaptureStackTrace=true and no exception → still null
            Assert.That(sink.Entries[1].StackTrace, Is.Null);
        }

        [Test]
        public void LogContext_StackTraceCapturedWhenCaptureStackTraceEnabled() {
            Logger.LogLevel = LogLevelDef.Debug;
            Logger.CaptureStackTrace = true;
            var sink = RegisterSink();

            Logger.Info("with-stack");

            Assert.That(sink.Entries[0].StackTrace, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void LogContext_MemberNameAndFilePathInjected() {
            Logger.LogLevel = LogLevelDef.Debug;
            var sink = RegisterSink();

            Logger.Info("caller-info-test");

            Assert.That(sink.Entries[0].MemberName, Is.Not.Null.And.Not.Empty);
            Assert.That(sink.Entries[0].FilePath, Is.Not.Null.And.Not.Empty);
            Assert.That(sink.Entries[0].LineNumber, Is.GreaterThan(0));
        }

        [Test]
        public void LogContext_MemberNameContainsTestMethodName() {
            Logger.LogLevel = LogLevelDef.Debug;
            var sink = RegisterSink();

            Logger.Info("method-name-check");

            Assert.That(sink.Entries[0].MemberName,
                Does.Contain("LogContext_MemberNameContainsTestMethodName"));
        }

        [Test]
        public void LogContext_FilePathContainsFileName() {
            Logger.LogLevel = LogLevelDef.Debug;
            var sink = RegisterSink();

            Logger.Info("file-path-check");

            Assert.That(sink.Entries[0].FilePath, Does.EndWith("LoggerBaselineTests.cs"));
        }

        [Test]
        public void VerboseFormatWithThreadIncludesThreadId() {
            Logger.LogLevel = LogLevelDef.Debug;
            Logger.ApplyConfiguration(new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = LogTemplates.Verbose
            });
            var sink = RegisterSink();

            Logger.Info("thread-test");

            var content = sink.Entries[0].Content;
            Assert.That(content, Does.Match(@"\[T:\d+\]"));
        }

        [Test]
        public void VerboseFormatWithLineIncludesLineNumber() {
            Logger.LogLevel = LogLevelDef.Debug;
            Logger.ApplyConfiguration(new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = LogTemplates.Verbose
            });
            var sink = RegisterSink();

            Logger.Info("line-test");

            var content = sink.Entries[0].Content;
            Assert.That(content, Does.Match(@"\[L:\d+\]"));
        }

        // ── Trace Level ────────────────────────────────────────

        [Test]
        public void TraceFilteredByDefaultLogLevel() {
            var sink = RegisterSink();
            // Default LogLevel is Error, so Trace should be blocked
            Logger.LogLevel = LogLevelDef.Error;

            Logger.Trace("trace-msg");
            Logger.Debug("debug-msg");

            Assert.That(sink.Entries.Count, Is.EqualTo(0));
        }

        [Test]
        public void TracePassesWhenLogLevelIsTrace() {
            var sink = RegisterSink();
            Logger.LogLevel = LogLevelDef.Trace;

            Logger.Trace("trace-msg");

            Assert.That(sink.Entries.Count, Is.EqualTo(1));
            Assert.That(sink.Entries[0].Level, Is.EqualTo(LogLevelDef.Trace));
        }

        // ── Per-Sink Level Filtering ───────────────────────────

        [Test]
        public void PerSinkLevel_SinkAtErrorOnlyReceivesErrorAndAbove() {
            Logger.LogLevel = LogLevelDef.Debug;
            var sink = RegisterSink(LogLevelDef.Error);

            Logger.Info("should-be-blocked");
            Logger.Warning("should-be-blocked");
            Logger.Error("should-pass");
            Logger.Fatal("should-pass");

            Assert.That(sink.Entries.Count, Is.EqualTo(2));
            Assert.That(sink.Entries[0].Level, Is.EqualTo(LogLevelDef.Error));
            Assert.That(sink.Entries[1].Level, Is.EqualTo(LogLevelDef.Fatal));
        }

        [Test]
        public void PerSinkLevel_DefaultMinLevelReceivesAll() {
            Logger.LogLevel = LogLevelDef.Trace;
            var sink = RegisterSink(LogLevelDef.Trace);

            Logger.Trace("trace");
            Logger.Debug("debug");
            Logger.Fatal("fatal");

            Assert.That(sink.Entries.Count, Is.EqualTo(3));
        }

        [Test]
        public void PerSinkLevel_MultipleSinksDifferentLevels() {
            Logger.LogLevel = LogLevelDef.Trace;
            var allSink = RegisterSink(LogLevelDef.Trace);
            var errorSink = RegisterSink(LogLevelDef.Error);

            Logger.Info("info");
            Logger.Error("error");

            Assert.That(allSink.Entries.Count, Is.EqualTo(2));
            Assert.That(errorSink.Entries.Count, Is.EqualTo(1));
            Assert.That(errorSink.Entries[0].Level, Is.EqualTo(LogLevelDef.Error));
        }

        // ── Helper ─────────────────────────────────────────────

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
