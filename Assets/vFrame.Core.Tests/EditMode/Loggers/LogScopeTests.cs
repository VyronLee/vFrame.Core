// ------------------------------------------------------------
//         File: LogScopeTests.cs
//        Brief: Tests for LogScope scoped property enrichment
//               and LogContextProperties integration.
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
    public class LogScopeTests
    {
        private readonly List<Logger.ILogSink> _registeredSinks = new List<Logger.ILogSink>();

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

            _registeredSinks.Clear();
            Logger.Close();
        }

        [Test]
        public void LogScope_PushProperty_AvailableInLogContext() {
            var sink = RegisterSink();

            using (new LogScope("RequestId", "req-001")) {
                Logger.Info("scoped-log");
            }

            Assert.That(sink.Entries.Count, Is.EqualTo(1));
            Assert.That(sink.Entries[0].Properties, Is.Not.Null);
            Assert.That(sink.Entries[0].Properties.ContainsKey("RequestId"), Is.True);
            Assert.That(sink.Entries[0].Properties["RequestId"], Is.EqualTo("req-001"));
        }

        [Test]
        public void LogScope_Dispose_RemovesProperty() {
            var sink = RegisterSink();

            using (new LogScope("SessionId", "sess-001")) {
                // Inside scope
            }

            Logger.Info("after-scope");

            Assert.That(sink.Entries.Count, Is.EqualTo(1));
            Assert.That(sink.Entries[0].Properties.ContainsKey("SessionId"), Is.False);
        }

        [Test]
        public void LogScope_NestedScopes_AllPropertiesAvailable() {
            var sink = RegisterSink();

            using (new LogScope("Outer", "outer-val")) {
                using (new LogScope("Inner", "inner-val")) {
                    Logger.Info("nested-scope");
                }
            }

            Assert.That(sink.Entries[0].Properties["Outer"], Is.EqualTo("outer-val"));
            Assert.That(sink.Entries[0].Properties["Inner"], Is.EqualTo("inner-val"));
        }

        [Test]
        public void LogScope_WithTemplateFormatter_PropertyTokenRendersValue() {
            var config = new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = "{level:u3} [req:{property:RequestId}] {message}"
            };
            Logger.ApplyConfiguration(config);
            var sink = RegisterSink();

            using (new LogScope("RequestId", "abc-123")) {
                Logger.Info("with-scope");
            }

            var content = sink.Entries[0].Content;
            Assert.That(content, Does.Contain("abc-123"));
            Assert.That(content, Does.Contain("with-scope"));
        }

        [Test]
        public void LogContext_WithoutScope_PropertiesEmpty() {
            var sink = RegisterSink();

            Logger.Info("no-scope");

            Assert.That(sink.Entries[0].Properties, Is.Not.Null);
            Assert.That(sink.Entries[0].Properties.Count, Is.EqualTo(0));
        }

        [Test]
        public void LogScope_OverwriteKey_ReplacesValue() {
            var sink = RegisterSink();

            using (new LogScope("Key", "first")) {
                using (new LogScope("Key", "second")) {
                    Logger.Info("overwrite");
                }
            }

            Assert.That(sink.Entries[0].Properties["Key"], Is.EqualTo("second"));
        }

        [Test]
        public void LogScope_ExceptionLog_StillCapturesProperties() {
            var sink = RegisterSink();

            using (new LogScope("CorrelationId", "corr-456")) {
                Logger.Error(new System.Exception("test"), "error-with-scope");
            }

            Assert.That(sink.Entries[0].Properties.ContainsKey("CorrelationId"), Is.True);
            Assert.That(sink.Entries[0].Properties["CorrelationId"], Is.EqualTo("corr-456"));
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
