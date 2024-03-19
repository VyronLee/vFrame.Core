//------------------------------------------------------------
//        File:  Logger.cs
//       Brief:  日志系统
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2018-10-20 18:09
//   Copyright:  Copyright (c) 2024, VyronLee
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

        private static readonly Queue<LogContext> _logQueue;
        private static readonly object _queueLock;
        private static string _logFilePath;
        private static LogToFile _logFile;

        private static readonly LogTag EmptyLogTag = new LogTag("__EMPTY__");

        static Logger() {
            _logQueue = new Queue<LogContext>(LogCapacity);
            _queueLock = new object();
        }

        public static string LogFilePath {
            set {
                _logFilePath = value;
                RecreateLogFile();
            }
        }

        public static event Action<LogContext> OnLogReceived;

        private static void RecreateLogFile() {
            Close();

            if (string.IsNullOrEmpty(_logFilePath)) {
                return;
            }

            _logFile = new LogToFile();
            _logFile.Create(_logFilePath);
            _logFile.AppendTimestamp = true;
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

        public static void Fatal(LogTag tag, Exception exception) {
            Log(LogLevelDef.Fatal, tag, exception);
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

        private static void Log(LogLevelDef level, LogTag tag, string text, params object[] args) {
            Log(1, level, tag, text, args);
        }

        private static void Log(int skip, LogLevelDef level, LogTag tag, string text, params object[] args) {
            if (LogLevel > level) {
                return;
            }

            var logText = args != null && args.Length > 0 ? string.Format(text, args) : text;
            var content = GetFormattedLogText(skip, tag, logText);
            var stack = GetLogStack(skip);

            var context = new LogContext(level, tag, content, stack, null);
            lock (_queueLock) {
                if (_logQueue.Count >= LogCapacity) {
                    _logQueue.Dequeue();
                }
                _logQueue.Enqueue(context);
            }

            _logFile?.AppendText(content);

            OnLogReceived?.Invoke(context);
        }

        private static void Log(LogLevelDef level, LogTag tag, Exception exception) {
            if (LogLevel > level) {
                return;
            }

            var context = new LogContext(level, tag, exception.Message, exception.StackTrace, exception);
            lock (_queueLock) {
                if (_logQueue.Count >= LogCapacity) {
                    _logQueue.Dequeue();
                }
                _logQueue.Enqueue(context);
            }

            _logFile?.AppendText(exception.ToString());

            OnLogReceived?.Invoke(context);
        }

        private static string GetFormattedLogText(int skip, LogTag tag, string log) {
            var builder = StringBuilderPool.Shared.Get();
            if ((LogFormatMask & LogFormatType.Tag) > 0 && !string.IsNullOrEmpty(LogTagFormatter) &&
                !tag.Equals(EmptyLogTag)) {
                try {
                    builder.Append(string.Format(LogTagFormatter, tag.ToString()));
                }
                catch (FormatException) { }
                builder.Append(" ");
            }

            if ((LogFormatMask & LogFormatType.Time) > 0) {
                builder.Append(DateTime.Now.ToString("[HH:mm:ss:fff]"));
                builder.Append(" ");
            }

            if ((LogFormatMask & LogFormatType.Class) > 0) {
                var stackFrame = new StackFrame(skip + 3);
                var methodBase = stackFrame.GetMethod();
                if (null != methodBase) {
                    if ((LogFormatMask & LogFormatType.Function) > 0) {
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
            else if ((LogFormatMask & LogFormatType.Function) > 0) {
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
            while (skip-- > 0) {
                stackTrace = stackTrace.Substring(stackTrace.IndexOf("\n", StringComparison.Ordinal) + 1);
            }
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

        public static void Fatal(Exception exception) {
            Fatal(EmptyLogTag, exception);
        }

        public static IEnumerable<LogContext> Logs(int logMask) {
            var logs = new Queue<LogContext>();

            lock (_queueLock) {
                foreach (var logContext in _logQueue) {
                    if (((int)logContext.Level & logMask) > 0) {
                        logs.Enqueue(logContext);
                    }
                }
            }

            return logs;
        }

        public struct LogContext
        {
            public LogLevelDef Level;
            public string Content;
            public LogTag Tag;
            public string StackTrace;
            public Exception Exception;

            public LogContext(LogLevelDef level, LogTag tag, string content, string stackTrace,
                Exception exception) : this() {
                Level = level;
                Content = content;
                Tag = tag;
                StackTrace = stackTrace;
                Exception = exception;
            }
        }

        #region Properties

        public static LogLevelDef LogLevel { get; set; } = LogLevelDef.Error;

        public static int LogFormatMask { get; set; } = DefaultLogFormatMask;

        public static string LogTagFormatter { get; set; } = DefaultTagFormatter;

        public static int LogCapacity { get; set; } = DefaultCapacity;

        #endregion
    }
}