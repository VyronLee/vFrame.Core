// ------------------------------------------------------------
//         File: Logger.cs
//        Brief: Central logging system with buffered log queue,
//               multiple sinks, and configurable format output.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2018-10-20 18:09:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using vFrame.Core;

namespace vFrame.Core
{
    public static class Logger
    {
        public interface ILogSink
        {
            /// <summary>
            /// Called when a new log context is received.
            /// </summary>
            /// <param name="context">The log context containing level, tag, content, and optional stack trace.</param>
            void OnLogReceived(LogContext context);
        }

        public const int DefaultCapacity = 1000;
        public const string DefaultTagFormatter = "{0}";

        public const int DefaultLogFormatMask =
            LogFormatType.Tag | LogFormatType.Time | LogFormatType.Class | LogFormatType.Function;

        private static readonly Queue<LogContext> _logQueue;
        private static readonly List<ILogSink> _sinks = new List<ILogSink>();
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

        /// <summary>
        /// Registers a lightweight log sink that receives all dispatched log contexts.
        /// </summary>
        /// <param name="sink">The sink to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sink"/> is null.</exception>
        public static void AddSink(ILogSink sink) {
            if (sink == null) {
                throw new ArgumentNullException(nameof(sink));
            }

            lock (_queueLock) {
                if (_sinks.Contains(sink)) {
                    return;
                }

                _sinks.Add(sink);
            }
        }

        /// <summary>
        /// Removes a previously registered lightweight log sink.
        /// </summary>
        /// <param name="sink">The sink to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sink"/> is null.</exception>
        public static void RemoveSink(ILogSink sink) {
            if (sink == null) {
                throw new ArgumentNullException(nameof(sink));
            }

            lock (_queueLock) {
                _sinks.Remove(sink);
            }
        }

        /// <summary>
        /// Returns the current number of registered lightweight sinks.
        /// </summary>
        /// <returns>The number of registered sinks.</returns>
        public static int GetSinkCount() {
            lock (_queueLock) {
                return _sinks.Count;
            }
        }

        /// <summary>
        /// Closes the current log file if one is open.
        /// </summary>
        private static void RecreateLogFile() {
            Close();

            if (string.IsNullOrEmpty(_logFilePath)) {
                return;
            }

            _logFile = new LogToFile();
            _logFile.Create(_logFilePath);
            _logFile.AppendTimestamp = true;
        }

        /// <summary>
        /// Closes and disposes the current log file.
        /// </summary>
        public static void Close() {
            _logFile?.Destroy();
            _logFile = null;
        }

        /// <summary>
        /// Logs a debug-level message with the specified tag.
        /// </summary>
        /// <param name="tag">The log tag identifying the source.</param>
        /// <param name="text">The format string for the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Debug(LogTag tag, string text, params object[] args) {
            Log(LogLevelDef.Debug, tag, text, args);
        }

        /// <summary>
        /// Logs an info-level message with the specified tag.
        /// </summary>
        /// <param name="tag">The log tag identifying the source.</param>
        /// <param name="text">The format string for the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Info(LogTag tag, string text, params object[] args) {
            Log(LogLevelDef.Info, tag, text, args);
        }

        /// <summary>
        /// Logs a warning-level message with the specified tag.
        /// </summary>
        /// <param name="tag">The log tag identifying the source.</param>
        /// <param name="text">The format string for the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Warning(LogTag tag, string text, params object[] args) {
            Log(LogLevelDef.Warning, tag, text, args);
        }

        /// <summary>
        /// Logs an error-level message with the specified tag.
        /// </summary>
        /// <param name="tag">The log tag identifying the source.</param>
        /// <param name="text">The format string for the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Error(LogTag tag, string text, params object[] args) {
            Log(LogLevelDef.Error, tag, text, args);
        }

        /// <summary>
        /// Logs a fatal-level message with the specified tag.
        /// </summary>
        /// <param name="tag">The log tag identifying the source.</param>
        /// <param name="text">The format string for the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Fatal(LogTag tag, string text, params object[] args) {
            Log(LogLevelDef.Fatal, tag, text, args);
        }

        /// <summary>
        /// Logs a fatal-level exception with the specified tag.
        /// </summary>
        /// <param name="tag">The log tag identifying the source.</param>
        /// <param name="exception">The exception to log.</param>
        public static void Fatal(LogTag tag, Exception exception) {
            Log(LogLevelDef.Fatal, tag, exception);
        }

        /// <summary>
        /// Logs a debug-level message with additional stack frame skip count.
        /// </summary>
        /// <param name="skip">Number of additional stack frames to skip when capturing caller info.</param>
        /// <param name="tag">The log tag identifying the source.</param>
        /// <param name="text">The format string for the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Debug(int skip, LogTag tag, string text, params object[] args) {
            Log(skip, LogLevelDef.Debug, tag, text, args);
        }

        /// <summary>
        /// Logs an info-level message with additional stack frame skip count.
        /// </summary>
        /// <param name="skip">Number of additional stack frames to skip when capturing caller info.</param>
        /// <param name="tag">The log tag identifying the source.</param>
        /// <param name="text">The format string for the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Info(int skip, LogTag tag, string text, params object[] args) {
            Log(skip, LogLevelDef.Info, tag, text, args);
        }

        /// <summary>
        /// Logs a warning-level message with additional stack frame skip count.
        /// </summary>
        /// <param name="skip">Number of additional stack frames to skip when capturing caller info.</param>
        /// <param name="tag">The log tag identifying the source.</param>
        /// <param name="text">The format string for the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Warning(int skip, LogTag tag, string text, params object[] args) {
            Log(skip, LogLevelDef.Warning, tag, text, args);
        }

        /// <summary>
        /// Logs an error-level message with additional stack frame skip count.
        /// </summary>
        /// <param name="skip">Number of additional stack frames to skip when capturing caller info.</param>
        /// <param name="tag">The log tag identifying the source.</param>
        /// <param name="text">The format string for the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Error(int skip, LogTag tag, string text, params object[] args) {
            Log(skip, LogLevelDef.Error, tag, text, args);
        }

        /// <summary>
        /// Logs a message at the specified level with default stack frame skip.
        /// </summary>
        private static void Log(LogLevelDef level, LogTag tag, string text, params object[] args) {
            Log(1, level, tag, text, args);
        }

        /// <summary>
        /// Core logging method. Formats the message, captures the stack trace, enqueues the context,
        /// writes to the log file, and dispatches to sinks.
        /// </summary>
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
            EmitToSinks(context);
        }

        /// <summary>
        /// Logs an exception at the specified level. Enqueues the context, writes to the log file,
        /// and dispatches to sinks.
        /// </summary>
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
            EmitToSinks(context);
        }

        /// <summary>
        /// Dispatches the log context to all currently registered sinks.
        /// </summary>
        private static void EmitToSinks(LogContext context) {
            ILogSink[] sinks;

            lock (_queueLock) {
                if (_sinks.Count == 0) {
                    return;
                }

                sinks = _sinks.ToArray();
            }

            foreach (var sink in sinks) {
                sink.OnLogReceived(context);
            }
        }

        /// <summary>
        /// Builds a formatted log string based on the current <see cref="LogFormatMask"/>.
        /// Includes optional tag, timestamp, class name, and function name.
        /// </summary>
        /// <returns>The formatted log text.</returns>
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

        /// <summary>
        /// Extracts and trims the stack trace, skipping the specified number of internal frames.
        /// </summary>
        /// <returns>The trimmed stack trace string.</returns>
        private static string GetLogStack(int skip) {
            var stackTrace = StackTraceUtility.ExtractStackTrace();
            skip += 3;
            while (skip-- > 0) {
                stackTrace = stackTrace.Substring(stackTrace.IndexOf("\n", StringComparison.Ordinal) + 1);
            }
            return stackTrace;
        }

        /// <summary>
        /// Logs a debug-level message without a tag.
        /// </summary>
        /// <param name="text">The format string for the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Debug(string text, params object[] args) {
            Log(LogLevelDef.Debug, EmptyLogTag, text, args);
        }

        /// <summary>
        /// Logs an info-level message without a tag.
        /// </summary>
        /// <param name="text">The format string for the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Info(string text, params object[] args) {
            Log(LogLevelDef.Info, EmptyLogTag, text, args);
        }

        /// <summary>
        /// Logs a warning-level message without a tag.
        /// </summary>
        /// <param name="text">The format string for the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Warning(string text, params object[] args) {
            Log(LogLevelDef.Warning, EmptyLogTag, text, args);
        }

        /// <summary>
        /// Logs an error-level message without a tag.
        /// </summary>
        /// <param name="text">The format string for the log message.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void Error(string text, params object[] args) {
            Log(LogLevelDef.Error, EmptyLogTag, text, args);
        }

        /// <summary>
        /// Logs a fatal-level exception without a tag.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        public static void Fatal(Exception exception) {
            Fatal(EmptyLogTag, exception);
        }

        /// <summary>
        /// Returns buffered log contexts matching the specified level mask.
        /// </summary>
        /// <param name="logMask">A bitmask matching <see cref="LogLevelDef"/> values.</param>
        /// <returns>A collection of matching log contexts.</returns>
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

        /// <summary>
        /// Returns the current number of buffered log entries.
        /// </summary>
        /// <returns>The number of entries in the log queue.</returns>
        public static int GetBufferedLogCount() {
            lock (_queueLock) {
                return _logQueue.Count;
            }
        }

        public struct LogContext
        {
            public LogLevelDef Level;
            public string Content;
            public LogTag Tag;
            public string StackTrace;
            public Exception Exception;

            /// <summary>
            /// Creates a new log context with the specified fields.
            /// </summary>
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
