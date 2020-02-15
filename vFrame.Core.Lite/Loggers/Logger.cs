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
        private static LogLevelDef _level = LogLevelDef.Error;
        private static int _capacity = LoggerSetting.Capacity;

        private static readonly Queue<LogContext> LogQueue;
        private static readonly object QueueLock;

        static Logger() {
            LogQueue = new Queue<LogContext>(_capacity);
            QueueLock = new object();
        }

        public static LogLevelDef LogLevel {
            get { return _level; }
            set { _level = value; }
        }

        public static int LogCapacity {
            get { return _capacity; }
            set { _capacity = value; }
        }

        public static event Action<LogContext> OnLogReceived;

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
            lock (QueueLock) {
                if (LogQueue.Count >= _capacity)
                    LogQueue.Dequeue();
                LogQueue.Enqueue(context);
            }

            OnLogReceived?.Invoke(context);
        }

        private static string GetFormattedLogText(int skip, LogTag tag, string log) {
            var stackFrame = new StackFrame(skip + 3);

            var builder = StringBuilderPool.Get();
            if ((LoggerSetting.LogFormatMask & LogFormatType.Tag) > 0) {
                builder.Append(string.Format(LoggerSetting.TagFormatter, tag.ToString()));
                builder.Append(" ");
            }

            if ((LoggerSetting.LogFormatMask & LogFormatType.Time) > 0) {
                builder.Append(DateTime.Now.ToString("[HH:mm:ss:fff]"));
                builder.Append(" ");
            }

            if ((LoggerSetting.LogFormatMask & LogFormatType.Class) > 0) {
                if ((LoggerSetting.LogFormatMask & LogFormatType.Function) > 0) {
                    builder.Append("[");
                    builder.Append(stackFrame.GetMethod().ReflectedType.Name);
                    builder.Append("::");
                    builder.Append(stackFrame.GetMethod().Name);
                    builder.Append("]");
                }
                else {
                    builder.Append("[");
                    builder.Append(stackFrame.GetMethod().ReflectedType.Name);
                    builder.Append("]");
                }
            }
            else if ((LoggerSetting.LogFormatMask & LogFormatType.Function) > 0) {
                builder.Append("[");
                builder.Append(stackFrame.GetMethod().Name);
                builder.Append("]");
            }

            builder.Append(" ");
            builder.Append(log);

            var text = builder.ToString();
            StringBuilderPool.Return(builder);

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
            Log(LogLevelDef.Debug, new LogTag(), text, args);
        }

        public static void Info(string text, params object[] args) {
            Log(LogLevelDef.Info, new LogTag(), text, args);
        }

        public static void Warning(string text, params object[] args) {
            Log(LogLevelDef.Warning, new LogTag(), text, args);
        }

        public static void Error(string text, params object[] args) {
            Log(LogLevelDef.Error, new LogTag(), text, args);
        }

        public static IEnumerable<LogContext> Logs(int logMask) {
            var logs = new Queue<LogContext>();

            lock (QueueLock) {
                foreach (var logContext in LogQueue)
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