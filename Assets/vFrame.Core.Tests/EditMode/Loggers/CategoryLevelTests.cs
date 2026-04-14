// ------------------------------------------------------------
//         File: CategoryLevelTests.cs
//        Brief: Tests for per-category log level configuration,
//               including timing of ApplyConfiguration vs GetLogger.
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
    public class CategoryLevelTests
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
        public void ApplyConfigBeforeGetLogger_CategoryLevelStillApplied() {
            // Apply config BEFORE creating any category logger
            var config = new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                CategoryLevels = new Dictionary<string, LogLevelDef> {
                    { "MyApp.Network", LogLevelDef.Error }
                }
            };
            Logger.ApplyConfiguration(config);

            // Now create the category logger
            var logger = Logger.GetLogger("MyApp.Network") as LoggerCategory;
            Assert.That(logger, Is.Not.Null);
            Assert.That(logger.MinimumLevel, Is.EqualTo(LogLevelDef.Error));
        }

        [Test]
        public void ApplyConfigAfterGetLogger_CategoryLevelAppliedImmediately() {
            // Create category logger first
            var logger = Logger.GetLogger("MyApp.Audio") as LoggerCategory;

            // Then apply config
            var config = new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                CategoryLevels = new Dictionary<string, LogLevelDef> {
                    { "MyApp.Audio", LogLevelDef.Warning }
                }
            };
            Logger.ApplyConfiguration(config);

            Assert.That(logger.MinimumLevel, Is.EqualTo(LogLevelDef.Warning));
        }

        [Test]
        public void PrefixMatching_AppliesToSubcategories() {
            var config = new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                CategoryLevels = new Dictionary<string, LogLevelDef> {
                    { "MyApp", LogLevelDef.Error }
                }
            };
            Logger.ApplyConfiguration(config);

            // Subcategory created after config
            var subLogger = Logger.GetLogger("MyApp.Network.Tcp") as LoggerCategory;
            Assert.That(subLogger.MinimumLevel, Is.EqualTo(LogLevelDef.Error));
        }

        [Test]
        public void ReapplyConfig_UpdatesExistingCategories() {
            var logger = Logger.GetLogger("MyApp.Core") as LoggerCategory;

            var config1 = new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                CategoryLevels = new Dictionary<string, LogLevelDef> {
                    { "MyApp.Core", LogLevelDef.Warning }
                }
            };
            Logger.ApplyConfiguration(config1);
            Assert.That(logger.MinimumLevel, Is.EqualTo(LogLevelDef.Warning));

            var config2 = new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                CategoryLevels = new Dictionary<string, LogLevelDef> {
                    { "MyApp.Core", LogLevelDef.Trace }
                }
            };
            Logger.ApplyConfiguration(config2);
            Assert.That(logger.MinimumLevel, Is.EqualTo(LogLevelDef.Trace));
        }

        [Test]
        public void CategoryLogger_RespectsItsMinimumLevel() {
            var sink = RegisterSink();
            var config = new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                CategoryLevels = new Dictionary<string, LogLevelDef> {
                    { "Filtered", LogLevelDef.Error }
                }
            };
            Logger.ApplyConfiguration(config);

            var logger = Logger.GetLogger("Filtered");
            logger.Info("should-be-filtered");
            logger.Error("should-pass");

            Assert.That(sink.Entries.Count, Is.EqualTo(1));
            Assert.That(sink.Entries[0].Level, Is.EqualTo(LogLevelDef.Error));
        }

        [Test]
        public void GetLoggerGeneric_UsesTypeFullName() {
            var logger = Logger.GetLogger<CategoryLevelTests>();
            Assert.That(logger.CategoryName, Is.EqualTo(typeof(CategoryLevelTests).FullName));
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
