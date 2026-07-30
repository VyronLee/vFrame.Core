using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace vFrame.Core.Unity.TestRunner
{
    /// <summary>
    /// 基于 TestRunnerApi 的泛型无头 EditMode 测试执行器。在进程内跑测试（不走
    /// Unity Test Protocol 端口），因此即便 `-runTests` 被阻塞，也能在
    /// `Unity -batchmode -executeMethod` 下工作。各项目薄入口示例：
    ///   public static void Run() => HeadlessTestRunner.Run("My.Project.Tests.EditMode");
    /// 不传程序集名则跑项目内所有 EditMode 程序集。
    /// 退出码：0 = 全过，1 = 有失败，2 = 无测试。
    /// 结果：TestResults/headless-editmode-results.xml。
    /// </summary>
    public static class HeadlessTestRunner
    {
        private const string ResultsPath = "TestResults/headless-editmode-results.xml";

        public static void RunAllEditMode() => Run();

        public static void Run(params string[] assemblies)
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            var collector = new ResultCollector();
            api.RegisterCallbacks(collector);
            api.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = assemblies == null || assemblies.Length == 0 ? null : assemblies
            })
            {
                runSynchronously = true
            });
            Finish(collector);
        }

        private static void Finish(ResultCollector collector)
        {
            Directory.CreateDirectory("TestResults");
            File.WriteAllText(ResultsPath, collector.ToXml(), Encoding.UTF8);
            var total = collector.PassCount + collector.FailCount +
                        collector.SkipCount + collector.InconclusiveCount;
            Debug.Log("[HeadlessTestRunner] passed=" + collector.PassCount +
                      " failed=" + collector.FailCount +
                      " skipped=" + collector.SkipCount +
                      " inconclusive=" + collector.InconclusiveCount +
                      " total=" + total);
            if (total == 0)
            {
                Debug.LogError("[HeadlessTestRunner] No EditMode tests ran. " +
                               "Check the assembly name(s) and that the test asmdef compiles.");
                EditorApplication.Exit(2);
                return;
            }
            EditorApplication.Exit(collector.FailCount > 0 ? 1 : 0);
        }

        private sealed class ResultCollector : ICallbacks
        {
            private readonly List<string> _failures = new List<string>();
            public int PassCount, FailCount, SkipCount, InconclusiveCount;

            public void RunStarted(ITestAdaptor testsToRun) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                PassCount = result.PassCount;
                FailCount = result.FailCount;
                SkipCount = result.SkipCount;
                InconclusiveCount = result.InconclusiveCount;
            }

            public void TestStarted(ITestAdaptor test) { }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.Test.IsSuite) return;
                if (result.TestStatus == TestStatus.Failed)
                {
                    _failures.Add("[" + result.ResultState + "] " + result.FullName + "\n" +
                                  result.Message + "\n" + result.StackTrace);
                }
            }

            public string ToXml()
            {
                var sb = new StringBuilder();
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sb.AppendLine("<test-results>");
                sb.AppendLine("  <assembly passed=\"" + PassCount + "\" failed=\"" + FailCount +
                              "\" skipped=\"" + SkipCount + "\" inconclusive=\"" + InconclusiveCount + "\" />");
                foreach (var failure in _failures)
                {
                    sb.AppendLine("  <failure>");
                    sb.AppendLine("    " + SecurityElement.Escape(failure));
                    sb.AppendLine("  </failure>");
                }
                sb.AppendLine("</test-results>");
                return sb.ToString();
            }
        }
    }
}
