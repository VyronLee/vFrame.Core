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
using UnityEngine;

namespace vFrame.Core.Loggers
{
    public static class Logger
    {
        private static LogLevelDef _level = LogLevelDef.Error;
        private static int _capacity = LoggerSetting.DefaultCapacity;

        private static readonly Queue<LogContext> LogQueue;
        private static readonly object QueueLock;

        static Logger()
        {
            LogQueue = new Queue<LogContext>(_capacity);
            QueueLock = new object();
        }

        public static LogLevelDef LogLevel
        {
            get { return _level; }
            set { _level = value; }
        }

        public static int LogCapacity
        {
            get { return _capacity; }
            set { _capacity = value; }
        }

        public static event Action<LogContext> OnLogReceived;

        public static void Debug(LogTag tag, string text, params object[] args)
        {
            Log(LogLevelDef.Debug, tag, text, args);
        }

        public static void Info(LogTag tag, string text, params object[] args)
        {
            Log(LogLevelDef.Info, tag, text, args);
        }

        public static void Warning(LogTag tag, string text, params object[] args)
        {
            Log(LogLevelDef.Warning, tag, text, args);
        }

        public static void Error(LogTag tag, string text, params object[] args)
        {
            Log(LogLevelDef.Error, tag, text, args);
        }

        public static void Fatal(LogTag tag, string text, params object[] args)
        {
            Log(LogLevelDef.Fatal, tag, text, args);
        }

        private static void Log(LogLevelDef level, LogTag tag, string text, params object[] args)
        {
            if (_level > level)
                return;

            var logText = args != null && args.Length > 0 ? string.Format(text, args) : text;

            var stackTrace = StackTraceUtility.ExtractStackTrace();
            // Remove first two lines, Log() and LogInfo()/LogWarning(), etc.
            stackTrace = stackTrace.Substring(stackTrace.IndexOf("\n", StringComparison.Ordinal) + 1);
            stackTrace = stackTrace.Substring(stackTrace.IndexOf("\n", StringComparison.Ordinal) + 1);

            var stackFrame = new StackFrame(2);

            var content = string.Format("{0} [{1:HH:mm:ss:fff}] [{2}::{3}] {4}",
                string.Format(LoggerSetting.DefaultTagFormatter, tag),
                DateTime.Now,
                stackFrame.GetMethod().ReflectedType,
                stackFrame.GetMethod().Name,
                logText);

            var context = new LogContext(level, tag, content, stackTrace);
            lock (QueueLock)
            {
                if (LogQueue.Count >= _capacity)
                    LogQueue.Dequeue();
                LogQueue.Enqueue(context);
            }

            switch (level)
            {
                case LogLevelDef.Debug:
                case LogLevelDef.Info:
                    UnityEngine.Debug.Log(content);
                    break;
                case LogLevelDef.Warning:
                    UnityEngine.Debug.LogWarning(content);
                    break;
                case LogLevelDef.Error:
                case LogLevelDef.Fatal:
                    UnityEngine.Debug.LogError(content);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("level", level, null);
            }

            if (OnLogReceived != null)
                OnLogReceived(context);
        }

        public static void Debug(string text, params object[] args)
        {
            Log(LogLevelDef.Debug, new LogTag(), text, args);
        }

        public static void Info(string text, params object[] args)
        {
            Log(LogLevelDef.Info, new LogTag(), text, args);
        }

        public static void Warning(string text, params object[] args)
        {
            Log(LogLevelDef.Warning, new LogTag(), text, args);
        }

        public static void Error(string text, params object[] args)
        {
            Log(LogLevelDef.Error, new LogTag(), text, args);
        }

        public static IEnumerable<LogContext> Logs(int logMask)
        {
            var logs = new Queue<LogContext>();

            lock (QueueLock)
            {
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

            public LogContext(LogLevelDef level, LogTag tag, string content, string stackTrace) : this()
            {
                Level = level;
                Content = content;
                Tag = tag;
                StackTrace = stackTrace;
            }
        }
    }
}