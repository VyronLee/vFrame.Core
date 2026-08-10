// ------------------------------------------------------------
//         File: LogFormatterTests.cs
//        Brief: Tests for template-based log formatting
//               integration with the Logger pipeline.
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
    public class LogFormatterTests
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

        [Test]
        public void CompactTemplate_OverridesDefaultTemplate_WhenFormatTemplateSet() {
            var config = new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = LogTemplates.Compact
            };
            Logger.ApplyConfiguration(config);
            var sink = RegisterSink();

            Logger.Info(new LogTag("MyTag"), "hello");

            Assert.That(sink.Entries.Count, Is.EqualTo(1));
            var content = sink.Entries[0].Content;
            Assert.That(content, Does.Contain("INF"));
            Assert.That(content, Does.Contain("hello"));
            Assert.That(content, Does.Not.Match(@"\[\d{4}-\d{2}-\d{2}"));
            Assert.That(content, Does.Not.Contain("MyTag"));
        }

        [Test]
        public void DefaultTemplate_IsUsed_WhenNoTemplateSet() {
            var config = new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug
            };
            Logger.ApplyConfiguration(config);
            var sink = RegisterSink();

            Logger.Info(new LogTag("Tag1"), "default-template-test");

            Assert.That(sink.Entries.Count, Is.EqualTo(1));
            var content = sink.Entries[0].Content;
            Assert.That(content, Does.Contain("Tag1"));
            Assert.That(content, Does.Match(@"\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\]"));
        }

        [Test]
        public void LogTemplates_Default_ProducesExpectedOutput() {
            var config = new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = LogTemplates.Default
            };
            Logger.ApplyConfiguration(config);
            var sink = RegisterSink();

            Logger.Info(new LogTag("TestTag"), "msg");

            var content = sink.Entries[0].Content;
            Assert.That(content, Does.Match(@"\[\d{4}-\d{2}-\d{2}"));
            Assert.That(content, Does.Contain("INF"));
            Assert.That(content, Does.Contain("TestTag"));
            Assert.That(content, Does.Contain("msg"));
        }

        [Test]
        public void LogTemplates_Verbose_IncludesThreadAndLine() {
            var config = new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = LogTemplates.Verbose
            };
            Logger.ApplyConfiguration(config);
            var sink = RegisterSink();

            Logger.Info(new LogTag("VTag"), "verbose-msg");

            var content = sink.Entries[0].Content;
            Assert.That(content, Does.Match(@"\[T:\d+\]"));
            Assert.That(content, Does.Match(@"\d+")); // line number
        }

        [Test]
        public void ClearTemplate_RestoresDefaultTemplateFormatting() {
            var configWithTemplate = new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = LogTemplates.Compact
            };
            Logger.ApplyConfiguration(configWithTemplate);

            var configNoTemplate = new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug
            };
            Logger.ApplyConfiguration(configNoTemplate);
            var sink = RegisterSink();

            Logger.Info(new LogTag("RestoreTag"), "restored");

            var content = sink.Entries[0].Content;
            Assert.That(content, Does.Contain("RestoreTag"));
            Assert.That(content, Does.Match(@"\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\]"));
        }

        [Test]
        public void CustomTemplate_WithPropertyToken_RendersScopeProperties() {
            var config = new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = "{level:u3} [{property:RequestId}] {message}"
            };
            Logger.ApplyConfiguration(config);
            var sink = RegisterSink();

            using (new LogScope("RequestId", "req-123")) {
                Logger.Info("with-property");
            }

            var content = sink.Entries[0].Content;
            Assert.That(content, Does.Contain("req-123"));
            Assert.That(content, Does.Contain("with-property"));
        }

        [Test]
        public void ExceptionToken_RendersInTemplate() {
            var config = new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = "{level:u3} {message} {exception}"
            };
            Logger.ApplyConfiguration(config);
            var sink = RegisterSink();

            Logger.Error(new LogTag("ErrTag"),
                new System.Exception("test-exception"), "error-msg");

            Assert.That(sink.Entries.Count, Is.EqualTo(1));
            var content = sink.Entries[0].Content;
            Assert.That(content, Does.Contain("ERR"));
            Assert.That(content, Does.Contain("test-exception"));
        }

        [Test]
        public void TagToken_OmitsEmptySentinelFromTemplateOutput() {
            Logger.ApplyConfiguration(new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = "[{tag}] {message}"
            });
            var sink = RegisterSink();

            Logger.Info("untagged");

            Assert.That(sink.Entries.Count, Is.EqualTo(1));
            Assert.That(sink.Entries[0].Content, Is.EqualTo("[] untagged"));
            Assert.That(sink.Entries[0].Tag.ToString(), Is.EqualTo("__EMPTY__"));
        }

        [Test]
        public void TagToken_RendersCallerSuppliedTagNamedLikeEmptySentinel() {
            Logger.ApplyConfiguration(new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = "[{tag}] {message}"
            });
            var sink = RegisterSink();

            Logger.Info(new LogTag("__EMPTY__"), "tagged");

            Assert.That(sink.Entries.Count, Is.EqualTo(1));
            Assert.That(sink.Entries[0].Content, Is.EqualTo("[__EMPTY__] tagged"));
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
