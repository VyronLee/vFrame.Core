// ------------------------------------------------------------
//         File: UnityLoggerTests.cs
//        Brief: Tests for Unity Console logging bridge behavior.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-08-10
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using vFrame.Core.Unity;

namespace vFrame.Core.Tests.EditMode.Asynchronous
{
    [TestFixture]
    public class UnityLoggerTests
    {
        [SetUp]
        public void SetUp() {
            UnityLogger.Close();
            Logger.LogLevel = LogLevelDef.Debug;
            Logger.ApplyConfiguration(new LogConfiguration {
                GlobalMinimumLevel = LogLevelDef.Debug,
                FormatTemplate = "{message}"
            });
            Logger.CaptureStackTrace = false;
        }

        [TearDown]
        public void TearDown() {
            UnityLogger.Close();
        }

        [Test]
        public void OnLogReceived_DoesNotPrefixEmptyTagSentinel() {
            UnityLogger.Open(LogLevelDef.Debug, formatTemplate: "{message}");
            LogAssert.Expect(LogType.Log, new Regex(@"^(?!\[__EMPTY__\]).*console-message$"));

            Logger.Info("console-message");
        }
    }
}
