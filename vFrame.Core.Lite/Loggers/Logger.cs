//------------------------------------------------------------
//        File:  Logger.cs
//       Brief:  日志系统
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2018-10-20 18:09
//   Copyright:  Copyright (c) 2018, VyronLee
//============================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using vFrame.Core.ObjectPools.Builtin;

namespace vFrame.Core.Loggers
{
    public static class Logger
    {
        public const int DefaultCapacity = 1000;
        public const string DefaultTagFormatter = "{0}";
        public const int DefaultLogFormatMask =
            LogFormatType.Tag | LogFormatType.Time | LogFormatType.Class | LogFormatType.Function;

        private static LogLevelDef _level = LogLevelDef.Error;
        private static int _logFormatMask = DefaultLogFormatMask;
        private static string _tagFormatter = DefaultTagFormatter;
        private static int _capacity = DefaultCapacity;
        private static readonly Queue<LogContext> _logQueue;
        private static readonly object _queueLock;
        private static string _logFilePath;
        private static LogToFile _logFile;

        private static readonly LogTag EmptyLogTag = new LogTag("__EMPTY__");

        #region Properties
        public static LogLevelDef LogLevel {
            get => _level;
            set => _level = value;
        }

        public static int LogFormatMask {
            get => _logFormatMask;
            set => _logFormatMask = value;
        }

        public static string LogTagFormatter {
            get => _tagFormatter;
            set => _tagFormatter = value;
        }

        public static int LogCapacity {
            get => _capacity;
            set => _capacity = value;
        }
        #endregion

        static Logger() {
            _logQueue = new Queue<LogContext>(_capacity);
            _queueLock = new object();
        }

        public static event Action<LogContext> OnLogReceived;

        public static string LogFilePath {
            set {
                _logFilePath = value;
                RecreateLogFile();
            }
        }

        private static void RecreateLogFile() {
            Close();

            if (string.IsNullOrEmpty(_logFilePath)) {
                return;
            }

            _logFile = new LogToFile();
            _logFile.Create(_logFilePath);
        }

        public static void Close() {
            _logFile?.Destroy();
            _logFile = null;
        }

        public static void Debug(LogTag tag, string text, params object[] args) {
            Log(LogLevelDef.Debug, tag, text, args);
        }

        public static void Info(LogTag tag, string text, params object[] args) {
            Log(LogLevelDef.Info, tag, text, args);
        }

        public static void Warning(LogTag tag, string text, params object[] args) {
            Log(LogLevelDef.Warning, tag, text, args);
        }

        public static void Error(LogTag tag, string text, params object[] args) {
            Log(LogLevelDef.Error, tag, text, args);
        }

        public static void Fatal(LogTag tag, string text, params object[] args) {
            Log(LogLevelDef.Fatal, tag, text, args);
        }

        public static void Debug(int skip, LogTag tag, string text, params object[] args) {
            Log(skip, LogLevelDef.Debug, tag, text, args);
        }

        public static void Info(int skip, LogTag tag, string text, params object[] args) {
            Log(skip, LogLevelDef.Info, tag, text, args);
        }

        public static void Warning(int skip, LogTag tag, string text, params object[] args) {
            Log(skip, LogLevelDef.Warning, tag, text, args);
        }

        public static void Error(int skip, LogTag tag, string text, params object[] args) {
            Log(skip, LogLevelDef.Error, tag, text, args);
        }

        public static void Fatal(int skip, LogTag tag, string text, params object[] args) {
            Log(skip, LogLevelDef.Fatal, tag, text, args);
        }

        private static void Log(LogLevelDef level, LogTag tag, string text, params object[] args) {
            Log(1, level, tag, text, args);
        }

        private static void Log(int skip, LogLevelDef level, LogTag tag, string text, params object[] args) {
            if (_level > level)
                return;

            var logText = args != null && args.Length > 0 ? string.Format(text, args) : text;
            var content = GetFormattedLogText(skip, tag, logText);
            var stack = GetLogStack(skip);

            var context = new LogContext(level, tag, content, stack);
            lock (_queueLock) {
                if (_logQueue.Count >= _capacity)
                    _logQueue.Dequeue();
                _logQueue.Enqueue(context);
            }

            _logFile?.AppendText(content);

            OnLogReceived?.Invoke(context);
        }

        private static string GetFormattedLogText(int skip, LogTag tag, string log) {
            var builder = StringBuilderPool.Shared.Get();
            if ((_logFormatMask & LogFormatType.Tag) > 0 && !string.IsNullOrEmpty(_tagFormatter) && tag.Equals(EmptyLogTag)) {
                try {
                    builder.Append(string.Format(_tagFormatter, tag.ToString()));
                }
                catch (FormatException) {

                }
                builder.Append(" ");
            }

            if ((_logFormatMask & LogFormatType.Time) > 0) {
                builder.Append(DateTime.Now.ToString("[HH:mm:ss:fff]"));
                builder.Append(" ");
            }

            if ((_logFormatMask & LogFormatType.Class) > 0) {
                var stackFrame = new StackFrame(skip + 3);
                var methodBase = stackFrame.GetMethod();
                if (null != methodBase) {
                    if ((_logFormatMask & LogFormatType.Function) > 0) {
                        builder.Append("[");
                        builder.Append(null == methodBase.ReflectedType ? "<Unknown>" : methodBase.ReflectedType.Name);
                        builder.Append("::");
                        builder.Append(methodBase.Name);
                        builder.Append("]");
                    }
                    else {
                        builder.Append("[");
                        builder.Append(null == methodBase.ReflectedType ? "<Unknown>" : methodBase.ReflectedType.Name);
                        builder.Append("]");
                    }
                }
            }
            else if ((_logFormatMask & LogFormatType.Function) > 0) {
                var stackFrame = new StackFrame(skip + 3);
                var methodBase = stackFrame.GetMethod();
                if (null != methodBase) {
                    builder.Append("[");
                    builder.Append(methodBase.Name);
                    builder.Append("]");
                }
            }

            builder.Append(" ");
            builder.Append(log);

            var text = builder.ToString();
            StringBuilderPool.Shared.Return(builder);

            return text;
        }

        private static string GetLogStack(int skip) {
            var stackTrace = StackTraceUtility.ExtractStackTrace();
            // Remove first three lines, GetLogStack(), Log() and LogInfo()/LogWarning(), etc.
            skip += 3;
            while (skip-- > 0)
                stackTrace = stackTrace.Substring(stackTrace.IndexOf("\n", StringComparison.Ordinal) + 1);
            return stackTrace;
        }

        public static void Debug(string text, params object[] args) {
            Log(LogLevelDef.Debug, EmptyLogTag, text, args);
        }

        public static void Info(string text, params object[] args) {
            Log(LogLevelDef.Info, EmptyLogTag, text, args);
        }

        public static void Warning(string text, params object[] args) {
            Log(LogLevelDef.Warning, EmptyLogTag, text, args);
        }

        public static void Error(string text, params object[] args) {
            Log(LogLevelDef.Error, EmptyLogTag, text, args);
        }

        public static IEnumerable<LogContext> Logs(int logMask) {
            var logs = new Queue<LogContext>();

            lock (_queueLock) {
                foreach (var logContext in _logQueue)
                    if (((int) logContext.Level & logMask) > 0)
                        logs.Enqueue(logContext);
            }

            return logs;
        }

        public struct LogContext
        {
            public LogLevelDef Level;
            public string Content;
            public LogTag Tag;
            public string StackTrace;

            public LogContext(LogLevelDef level, LogTag tag, string content, string stackTrace) : this() {
                Level = level;
                Content = content;
                Tag = tag;
                StackTrace = stackTrace;
            }
        }
    }
}