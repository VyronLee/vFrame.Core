using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

public static class BatchTestRunner
{
    public static void RunEditMode() {
        Run(TestMode.EditMode);
    }

    public static void RunPlayMode() {
        Run(TestMode.PlayMode);
    }

    private static void Run(TestMode mode) {
        var arguments = Environment.GetCommandLineArgs();
        var outputPath = GetArgument(arguments, "-batchTestOutput");
        var testFilter = GetArgument(arguments, "-batchTestFilter");

        if (string.IsNullOrWhiteSpace(outputPath)) {
            throw new InvalidOperationException("Missing required -batchTestOutput argument.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");

        var runner = ScriptableObject.CreateInstance<TestRunnerApi>();
        var callbacks = new BatchTestCallbacks(outputPath, mode, testFilter);
        var filter = new Filter {
            testMode = mode,
            testNames = string.IsNullOrWhiteSpace(testFilter) ? null : new[] { testFilter }
        };

        runner.RegisterCallbacks(callbacks);
        runner.Execute(new ExecutionSettings(filter));
    }

    private static string GetArgument(string[] arguments, string name) {
        for (var i = 0; i < arguments.Length - 1; i++) {
            if (string.Equals(arguments[i], name, StringComparison.OrdinalIgnoreCase)) {
                return arguments[i + 1];
            }
        }

        return null;
    }

    private sealed class BatchTestCallbacks : ICallbacks
    {
        private readonly string _outputPath;

        private readonly TestMode _mode;

        private readonly string _testFilter;

        public BatchTestCallbacks(string outputPath, TestMode mode, string testFilter) {
            _outputPath = outputPath;
            _mode = mode;
            _testFilter = testFilter;
        }

        public int priority => 0;

        public void RunStarted(ITestAdaptor testsToRun) {
            UnityEngine.Debug.Log($"[BatchTestRunner] Started {_mode} run. Filter: {_testFilter ?? "<all>"}");
        }

        public void RunFinished(ITestResultAdaptor result) {
            var payload = new BatchTestResult {
                Mode = _mode.ToString(),
                Filter = _testFilter,
                Name = result.Name,
                PassCount = result.PassCount,
                FailCount = result.FailCount,
                SkipCount = result.SkipCount,
                InconclusiveCount = result.InconclusiveCount,
                ResultState = result.ResultState,
                Message = result.Message,
                Duration = result.Duration,
                TestCount = result.Test != null ? CountTests(result.Test) : 0,
                Failures = CollectFailures(result)
            };

            File.WriteAllText(_outputPath, JsonUtility.ToJson(payload, true));
            UnityEngine.Debug.Log($"[BatchTestRunner] Wrote results to {_outputPath}");
            EditorApplication.Exit(result.FailCount > 0 ? 1 : 0);
        }

        public void TestStarted(ITestAdaptor test) {
        }

        public void TestFinished(ITestResultAdaptor result) {
        }

        private static int CountTests(ITestAdaptor test) {
            if (test == null) {
                return 0;
            }

            if (test.IsTestAssembly || test.IsSuite) {
                var count = 0;

                foreach (var child in test.Children) {
                    count += CountTests(child);
                }

                return count;
            }

            return 1;
        }

        private static FailureInfo[] CollectFailures(ITestResultAdaptor result) {
            var failures = new List<FailureInfo>();
            CollectFailuresRecursive(result, failures);
            return failures.ToArray();
        }

        private static void CollectFailuresRecursive(ITestResultAdaptor result, List<FailureInfo> failures) {
            if (result == null) {
                return;
            }

            if (result.FailCount > 0 && result.Test != null && !result.HasChildren) {
                failures.Add(new FailureInfo {
                    Name = result.Name,
                    FullName = result.Test.FullName,
                    Message = result.Message,
                    StackTrace = result.StackTrace
                });
            }

            if (!result.HasChildren || result.Children == null) {
                return;
            }

            foreach (var child in result.Children) {
                CollectFailuresRecursive(child, failures);
            }
        }
    }

    [Serializable]
    private sealed class BatchTestResult
    {
        public string Mode;

        public string Filter;

        public string Name;

        public int TestCount;

        public int PassCount;

        public int FailCount;

        public int SkipCount;

        public int InconclusiveCount;

        public string ResultState;

        public string Message;

        public double Duration;

        public FailureInfo[] Failures;
    }

    [Serializable]
    private sealed class FailureInfo
    {
        public string Name;

        public string FullName;

        public string Message;

        public string StackTrace;
    }
}
