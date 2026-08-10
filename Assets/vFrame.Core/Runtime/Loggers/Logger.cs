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
using System.Runtime.CompilerServices;

namespace vFrame.Core
{
    public static class Logger
    {
        public const int DefaultCapacity = 1000;

        private static readonly Queue<LogContext> _logQueue;

        private static readonly List<(ILogSink sink, LogLevelDef minLevel)> _sinks =
            new List<(ILogSink, LogLevelDef)>();

        private static readonly List<(IStructuredLogSink sink, LogLevelDef minLevel)> _structuredSinks =
            new List<(IStructuredLogSink, LogLevelDef)>();

        private static readonly Dictionary<string, LoggerCategory> _categories =
            new Dictionary<string, LoggerCategory>();

        private static readonly Dictionary<string, LogLevelDef> _pendingCategoryLevels =
            new Dictionary<string, LogLevelDef>();

        private static readonly object _queueLock;
        private static string _logFilePath;
        private static LogToFile _logFile;

        private static readonly LogTag EmptyLogTag = new LogTag("__EMPTY__");

        private static LogFormatter _formatter;

        static Logger() {
            _logQueue = new Queue<LogContext>(LogCapacity);
            _queueLock = new object();
            _formatter = new LogFormatter(LogTemplates.Default);
        }

        public static string LogFilePath {
            set {
                _logFilePath = value;
                RecreateLogFile();
            }
        }

        public static event Action<LogContext> OnLogReceived;

        /// <summary>
        ///     Registers a lightweight log sink that receives log contexts at or above the specified level.
        /// </summary>
        /// <param name="sink">The sink to register.</param>
        /// <param name="minLevel">Minimum log level for this sink (default: Trace = receive all).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sink" /> is null.</exception>
        public static void AddSink(ILogSink sink, LogLevelDef minLevel = LogLevelDef.Trace) {
            if (sink == null) {
                throw new ArgumentNullException(nameof(sink));
            }

            lock (_queueLock) {
                for (var i = 0; i < _sinks.Count; i++) {
                    if (ReferenceEquals(_sinks[i].sink, sink)) {
                        _sinks[i] = (sink, minLevel);
                        return;
                    }
                }

                _sinks.Add((sink, minLevel));
            }
        }

        /// <summary>
        ///     Removes a previously registered lightweight log sink.
        /// </summary>
        /// <param name="sink">The sink to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sink" /> is null.</exception>
        public static void RemoveSink(ILogSink sink) {
            if (sink == null) {
                throw new ArgumentNullException(nameof(sink));
            }

            lock (_queueLock) {
                _sinks.RemoveAll(entry => ReferenceEquals(entry.sink, sink));
            }
        }

        /// <summary>
        ///     Returns the current number of registered lightweight sinks.
        /// </summary>
        /// <returns>The number of registered sinks.</returns>
        public static int GetSinkCount() {
            lock (_queueLock) {
                return _sinks.Count;
            }
        }

        /// <summary>
        ///     Registers a structured log sink that receives log contexts at or above the specified level.
        /// </summary>
        /// <param name="sink">The structured sink to register.</param>
        /// <param name="minLevel">Minimum log level for this sink (default: Trace = receive all).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sink" /> is null.</exception>
        public static void AddStructuredSink(IStructuredLogSink sink, LogLevelDef minLevel = LogLevelDef.Trace) {
            if (sink == null) {
                throw new ArgumentNullException(nameof(sink));
            }

            lock (_queueLock) {
                for (var i = 0; i < _structuredSinks.Count; i++) {
                    if (ReferenceEquals(_structuredSinks[i].sink, sink)) {
                        _structuredSinks[i] = (sink, minLevel);
                        return;
                    }
                }

                _structuredSinks.Add((sink, minLevel));
            }
        }

        /// <summary>
        ///     Removes a previously registered structured log sink.
        /// </summary>
        /// <param name="sink">The structured sink to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sink" /> is null.</exception>
        public static void RemoveStructuredSink(IStructuredLogSink sink) {
            if (sink == null) {
                throw new ArgumentNullException(nameof(sink));
            }

            lock (_queueLock) {
                _structuredSinks.RemoveAll(entry => ReferenceEquals(entry.sink, sink));
            }
        }

        /// <summary>
        ///     Closes the current log file if one is open.
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
        ///     Closes and disposes the current log file and clears the buffered
        ///     log queue. Callers (including test setup) treat <see cref="Close" />
        ///     as a full reset: failing to clear <c>_logQueue</c> here left stale
        ///     entries accumulating across fixtures when many tests share one
        ///     process, inflating buffered counts (runner-exposed, C-Logger).
        /// </summary>
        public static void Close() {
            lock (_queueLock) {
                _logQueue.Clear();
            }

            _logFile?.Destroy();
            _logFile = null;
            _formatter = new LogFormatter(LogTemplates.Default);
        }

        // ── Core log methods ──

        /// <summary>
        ///     Core logging method for formatted text. Builds the log context,
        ///     enqueues it, writes to the log file, and dispatches to sinks.
        /// </summary>
        private static void Log(LogLevelDef level, LogTag tag, string formattedText,
            string memberName, string filePath, int lineNumber) {
            if (LogLevel > level) {
                return;
            }

            var content = GetFormattedLogText(level, tag, formattedText, memberName, filePath, lineNumber);
            var stack = CaptureStackTrace && level >= LogLevelDef.Error
                ? GetLogStack()
                : null;

            var context = new LogContext(level, tag, content, formattedText, null, stack, null,
                memberName, filePath, lineNumber);
            EnqueueAndDispatch(context);
        }

        /// <summary>
        ///     Core logging method for exception messages with optional text.
        ///     Builds the log context, enqueues it, writes to the log file, and dispatches to sinks.
        /// </summary>
        private static void Log(LogLevelDef level, LogTag tag, Exception exception,
            string text, string memberName, string filePath, int lineNumber) {
            if (LogLevel > level) {
                return;
            }

            var message = string.IsNullOrEmpty(text)
                ? exception.Message
                : $"{text} — {exception.Message}";
            var content = GetFormattedLogText(level, tag, message, memberName, filePath, lineNumber);
            var stack = CaptureStackTrace
                ? GetLogStack()
                : exception?.StackTrace;

            var context = new LogContext(level, tag, content, message, null,
                stack, exception, memberName, filePath, lineNumber);
            EnqueueAndDispatch(context);
            _logFile?.AppendText(exception.ToString(), level >= LogLevelDef.Error);
        }

        /// <summary>
        ///     Core logging method for structured messages with separate template and args.
        ///     Formats the message via <c>string.Format</c>, populates <see cref="LogContext.Args" />,
        ///     enqueues, writes to file, and dispatches to sinks.
        /// </summary>
        private static void Log(LogLevelDef level, LogTag tag, string messageTemplate,
            object[] args, string memberName, string filePath, int lineNumber) {
            if (LogLevel > level) {
                return;
            }

            var formatted = args != null && args.Length > 0
                ? string.Format(messageTemplate, args)
                : messageTemplate;
            var content = GetFormattedLogText(level, tag, formatted, memberName, filePath, lineNumber);

            var context = new LogContext(level, tag, content, messageTemplate, args, null, null,
                memberName, filePath, lineNumber);
            EnqueueAndDispatch(context);
        }

        /// <summary>
        ///     Enqueues the context, writes to file, and dispatches to sinks.
        /// </summary>
        private static void EnqueueAndDispatch(LogContext context) {
            lock (_queueLock) {
                if (_logQueue.Count >= LogCapacity) {
                    _logQueue.Dequeue();
                }

                _logQueue.Enqueue(context);
            }

            _logFile?.AppendText(context.Content, context.Level >= LogLevelDef.Error);

            OnLogReceived?.Invoke(context);
            EmitToSinks(context);
        }

        // ── Sink dispatch ──

        /// <summary>
        ///     Dispatches the log context to all registered sinks whose minimum level matches.
        /// </summary>
        private static void EmitToSinks(LogContext context) {
            (ILogSink sink, LogLevelDef minLevel)[] sinks;
            (IStructuredLogSink sink, LogLevelDef minLevel)[] structuredSinks;

            lock (_queueLock) {
                sinks = _sinks.Count > 0 ? _sinks.ToArray() : null;
                structuredSinks = _structuredSinks.Count > 0 ? _structuredSinks.ToArray() : null;
            }

            if (sinks != null) {
                foreach (var (sink, minLevel) in sinks) {
                    if (context.Level >= minLevel) {
                        sink.OnLogReceived(context);
                    }
                }
            }

            if (structuredSinks != null) {
                foreach (var (sink, minLevel) in structuredSinks) {
                    if (context.Level >= minLevel) {
                        sink.OnLogReceived(context);
                    }
                }
            }
        }

        // ── Formatting ──

        /// <summary>
        ///     Builds a formatted log string by delegating to the active <see cref="LogFormatter" />.
        /// </summary>
        private static string GetFormattedLogText(LogLevelDef level, LogTag tag, string log,
            string memberName, string filePath, int lineNumber) {
            var ctx = new LogContext(level, tag, log, log, null, null, null,
                memberName, filePath, lineNumber);
            return _formatter.Format(ctx);
        }

        /// <summary>
        ///     Extracts the class name from a file path.
        ///     Handles both "Namespace.ClassName" and full file paths like "/path/to/ClassName.cs".
        /// </summary>
        private static string ExtractClassName(string filePath) {
            if (string.IsNullOrEmpty(filePath)) {
                return "<Unknown>";
            }

            // If the path contains directory separators, extract the file name without extension
            var lastSlash = filePath.LastIndexOfAny(new[] { '/', '\\' });
            if (lastSlash >= 0) {
                filePath = filePath.Substring(lastSlash + 1);
            }

            // Remove .cs extension if present
            if (filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) {
                filePath = filePath.Substring(0, filePath.Length - 3);
            }

            return filePath;
        }

        /// <summary>
        ///     Extracts the current stack trace, skipping internal Logger frames.
        ///     Only called when <see cref="CaptureStackTrace" /> is true or for Error/Fatal with exceptions.
        /// </summary>
        /// <returns>The trimmed stack trace string.</returns>
        private static string GetLogStack() {
            // Skip: GetLogStack → Log → public method
            return new StackTrace(3, true).ToString();
        }

        // ── Buffered log access ──

        /// <summary>
        ///     Returns buffered log contexts matching the specified level mask.
        /// </summary>
        /// <param name="logMask">A bitmask matching <see cref="LogLevelDef" /> values.</param>
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
        ///     Returns the current number of buffered log entries.
        /// </summary>
        /// <returns>The number of entries in the log queue.</returns>
        public static int GetBufferedLogCount() {
            lock (_queueLock) {
                return _logQueue.Count;
            }
        }

        // ── Category management ──

        /// <summary>
        ///     Returns a category-based logger for the specified category name.
        ///     Creates a new instance if one does not already exist.
        /// </summary>
        /// <param name="categoryName">The category name used as the log tag.</param>
        /// <returns>A logger instance for the category.</returns>
        public static ILogger GetLogger(string categoryName) {
            lock (_queueLock) {
                if (!_categories.TryGetValue(categoryName, out var logger)) {
                    logger = new LoggerCategory(categoryName);
                    _categories[categoryName] = logger;

                    // Apply pending category level if a matching prefix was configured
                    foreach (var kvp in _pendingCategoryLevels) {
                        if (categoryName.StartsWith(kvp.Key, StringComparison.Ordinal)) {
                            logger.MinimumLevel = kvp.Value;
                            break;
                        }
                    }
                }

                return logger;
            }
        }

        /// <summary>
        ///     Returns a category-based logger for the specified type's full name.
        ///     Creates a new instance if one does not already exist.
        /// </summary>
        /// <typeparam name="T">The type whose full name is used as the category.</typeparam>
        /// <returns>A logger instance for the type's category.</returns>
        public static ILogger GetLogger<T>() {
            return GetLogger(typeof(T).FullName ?? typeof(T).Name);
        }

        /// <summary>
        ///     Sets the minimum log level for all categories whose name starts with the specified prefix.
        /// </summary>
        /// <param name="prefix">The category name prefix to match.</param>
        /// <param name="level">The minimum log level for matching categories.</param>
        public static void SetLevel(string prefix, LogLevelDef level) {
            lock (_queueLock) {
                foreach (var entry in _categories) {
                    if (entry.Key.StartsWith(prefix, StringComparison.Ordinal)) {
                        entry.Value.MinimumLevel = level;
                    }
                }
            }
        }

        // ── Configuration ──

        /// <summary>
        ///     Applies a <see cref="LogConfiguration" /> to this logger instance.
        ///     Sets global level, per-category levels, file path, and other options.
        /// </summary>
        /// <param name="config">The configuration to apply.</param>
        public static void ApplyConfiguration(LogConfiguration config) {
            if (config == null) {
                return;
            }

            LogLevel = config.GlobalMinimumLevel;

            if (config.CategoryLevels != null) {
                _pendingCategoryLevels.Clear();
                foreach (var kvp in config.CategoryLevels) {
                    _pendingCategoryLevels[kvp.Key] = kvp.Value;
                }

                foreach (var kvp in config.CategoryLevels) {
                    SetLevel(kvp.Key, kvp.Value);
                }
            }

            if (!string.IsNullOrEmpty(config.FormatTemplate)) {
                _formatter = new LogFormatter(config.FormatTemplate);
            }
            else {
                _formatter = new LogFormatter(LogTemplates.Default);
            }

            if (!string.IsNullOrEmpty(config.FileLogPath)) {
                LogFilePath = config.FileLogPath;
                if (_logFile != null && config.FileLogOptions != null) {
                    _logFile.Configure(config.FileLogOptions);
                }
            }

            CaptureStackTrace = config.CaptureStackTrace;
        }

        public interface ILogSink
        {
            /// <summary>
            ///     Called when a new log context is received.
            /// </summary>
            /// <param name="context">The log context containing level, tag, content, and optional stack trace.</param>
            void OnLogReceived(LogContext context);
        }

        public interface IStructuredLogSink
        {
            /// <summary>
            ///     Called when a new log context is received for structured/JSON output.
            /// </summary>
            /// <param name="context">The log context containing all structured fields.</param>
            void OnLogReceived(LogContext context);
        }

        // ── LogContext ──

        public struct LogContext
        {
            public LogLevelDef Level;
            public LogTag Tag;
            public string Content;
            public string MessageTemplate;
            public object[] Args;
            public string StackTrace;
            public Exception Exception;
            public string MemberName;
            public string FilePath;
            public int LineNumber;
            public IReadOnlyDictionary<string, object> Properties;

            /// <summary>
            ///     Creates a new log context with all fields.
            ///     When <paramref name="properties" /> is null, captures the current
            ///     <see cref="LogContextProperties" /> snapshot automatically.
            /// </summary>
            public LogContext(LogLevelDef level, LogTag tag, string content,
                string messageTemplate, object[] args, string stackTrace,
                Exception exception, string memberName, string filePath, int lineNumber,
                IReadOnlyDictionary<string, object> properties = null) : this() {
                Level = level;
                Tag = tag;
                Content = content;
                MessageTemplate = messageTemplate;
                Args = args;
                StackTrace = stackTrace;
                Exception = exception;
                MemberName = memberName;
                FilePath = filePath;
                LineNumber = lineNumber;
                Properties = properties ?? LogContextProperties.GetCurrentProperties();
            }
        }

        // ── Tagged log methods ──

        #region Tagged Text

        /// <summary>Logs a trace-level plain text message with the specified tag.</summary>
        public static void Trace(LogTag tag, string text,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            if (LogLevel > LogLevelDef.Trace) {
                return;
            }

            Log(LogLevelDef.Trace, tag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a trace-level interpolated message. Zero-alloc when disabled.</summary>
        public static void Trace(LogTag tag, LogLevelDef level = LogLevelDef.Trace,
            [InterpolatedStringHandlerArgument("level")]
            LogInterpolatedStringHandler handler = default,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            Log(LogLevelDef.Trace, tag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a debug-level plain text message with the specified tag.</summary>
        public static void Debug(LogTag tag, string text,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            if (LogLevel > LogLevelDef.Debug) {
                return;
            }

            Log(LogLevelDef.Debug, tag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a debug-level interpolated message. Zero-alloc when disabled.</summary>
        public static void Debug(LogTag tag, LogLevelDef level = LogLevelDef.Debug,
            [InterpolatedStringHandlerArgument("level")]
            LogInterpolatedStringHandler handler = default,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            Log(LogLevelDef.Debug, tag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs an info-level plain text message with the specified tag.</summary>
        public static void Info(LogTag tag, string text,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            if (LogLevel > LogLevelDef.Info) {
                return;
            }

            Log(LogLevelDef.Info, tag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs an info-level interpolated message. Zero-alloc when disabled.</summary>
        public static void Info(LogTag tag, LogLevelDef level = LogLevelDef.Info,
            [InterpolatedStringHandlerArgument("level")]
            LogInterpolatedStringHandler handler = default,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            Log(LogLevelDef.Info, tag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a warning-level plain text message with the specified tag.</summary>
        public static void Warning(LogTag tag, string text,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            if (LogLevel > LogLevelDef.Warning) {
                return;
            }

            Log(LogLevelDef.Warning, tag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a warning-level interpolated message. Zero-alloc when disabled.</summary>
        public static void Warning(LogTag tag, LogLevelDef level = LogLevelDef.Warning,
            [InterpolatedStringHandlerArgument("level")]
            LogInterpolatedStringHandler handler = default,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            Log(LogLevelDef.Warning, tag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs an error-level plain text message with the specified tag.</summary>
        public static void Error(LogTag tag, string text,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            if (LogLevel > LogLevelDef.Error) {
                return;
            }

            Log(LogLevelDef.Error, tag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs an error-level interpolated message. Zero-alloc when disabled.</summary>
        public static void Error(LogTag tag, LogLevelDef level = LogLevelDef.Error,
            [InterpolatedStringHandlerArgument("level")]
            LogInterpolatedStringHandler handler = default,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            Log(LogLevelDef.Error, tag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a fatal-level plain text message with the specified tag.</summary>
        public static void Fatal(LogTag tag, string text,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            if (LogLevel > LogLevelDef.Fatal) {
                return;
            }

            Log(LogLevelDef.Fatal, tag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a fatal-level interpolated message. Zero-alloc when disabled.</summary>
        public static void Fatal(LogTag tag, LogLevelDef level = LogLevelDef.Fatal,
            [InterpolatedStringHandlerArgument("level")]
            LogInterpolatedStringHandler handler = default,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            Log(LogLevelDef.Fatal, tag, text, memberName, filePath, lineNumber);
        }

        #endregion

        #region Tagged Exception

        /// <summary>Logs a trace-level exception with the specified tag.</summary>
        public static void Trace(LogTag tag, Exception exception, string text = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Trace, tag, exception, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a debug-level exception with the specified tag.</summary>
        public static void Debug(LogTag tag, Exception exception, string text = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Debug, tag, exception, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs an info-level exception with the specified tag.</summary>
        public static void Info(LogTag tag, Exception exception, string text = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Info, tag, exception, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a warning-level exception with the specified tag.</summary>
        public static void Warning(LogTag tag, Exception exception, string text = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Warning, tag, exception, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs an error-level exception with the specified tag.</summary>
        public static void Error(LogTag tag, Exception exception, string text = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Error, tag, exception, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a fatal-level exception with the specified tag.</summary>
        public static void Fatal(LogTag tag, Exception exception, string text = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Fatal, tag, exception, text, memberName, filePath, lineNumber);
        }

        #endregion

        // ── Untagged log methods ──

        #region Untagged Text

        /// <summary>Logs a trace-level plain text message without a tag.</summary>
        public static void Trace(string text,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            if (LogLevel > LogLevelDef.Trace) {
                return;
            }

            Log(LogLevelDef.Trace, EmptyLogTag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a trace-level interpolated message without a tag. Zero-alloc when disabled.</summary>
        public static void Trace(LogLevelDef level = LogLevelDef.Trace,
            [InterpolatedStringHandlerArgument("level")]
            LogInterpolatedStringHandler handler = default,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            Log(LogLevelDef.Trace, EmptyLogTag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a debug-level plain text message without a tag.</summary>
        public static void Debug(string text,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            if (LogLevel > LogLevelDef.Debug) {
                return;
            }

            Log(LogLevelDef.Debug, EmptyLogTag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a debug-level interpolated message without a tag. Zero-alloc when disabled.</summary>
        public static void Debug(LogLevelDef level = LogLevelDef.Debug,
            [InterpolatedStringHandlerArgument("level")]
            LogInterpolatedStringHandler handler = default,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            Log(LogLevelDef.Debug, EmptyLogTag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs an info-level plain text message without a tag.</summary>
        public static void Info(string text,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            if (LogLevel > LogLevelDef.Info) {
                return;
            }

            Log(LogLevelDef.Info, EmptyLogTag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs an info-level interpolated message without a tag. Zero-alloc when disabled.</summary>
        public static void Info(LogLevelDef level = LogLevelDef.Info,
            [InterpolatedStringHandlerArgument("level")]
            LogInterpolatedStringHandler handler = default,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            Log(LogLevelDef.Info, EmptyLogTag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a warning-level plain text message without a tag.</summary>
        public static void Warning(string text,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            if (LogLevel > LogLevelDef.Warning) {
                return;
            }

            Log(LogLevelDef.Warning, EmptyLogTag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a warning-level interpolated message without a tag. Zero-alloc when disabled.</summary>
        public static void Warning(LogLevelDef level = LogLevelDef.Warning,
            [InterpolatedStringHandlerArgument("level")]
            LogInterpolatedStringHandler handler = default,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            Log(LogLevelDef.Warning, EmptyLogTag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs an error-level plain text message without a tag.</summary>
        public static void Error(string text,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            if (LogLevel > LogLevelDef.Error) {
                return;
            }

            Log(LogLevelDef.Error, EmptyLogTag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs an error-level interpolated message without a tag. Zero-alloc when disabled.</summary>
        public static void Error(LogLevelDef level = LogLevelDef.Error,
            [InterpolatedStringHandlerArgument("level")]
            LogInterpolatedStringHandler handler = default,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            Log(LogLevelDef.Error, EmptyLogTag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a fatal-level plain text message without a tag.</summary>
        public static void Fatal(string text,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            if (LogLevel > LogLevelDef.Fatal) {
                return;
            }

            Log(LogLevelDef.Fatal, EmptyLogTag, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a fatal-level interpolated message without a tag. Zero-alloc when disabled.</summary>
        public static void Fatal(LogLevelDef level = LogLevelDef.Fatal,
            [InterpolatedStringHandlerArgument("level")]
            LogInterpolatedStringHandler handler = default,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            Log(LogLevelDef.Fatal, EmptyLogTag, text, memberName, filePath, lineNumber);
        }

        #endregion

        #region Untagged Exception

        /// <summary>Logs a trace-level exception without a tag.</summary>
        public static void Trace(Exception exception, string text = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Trace, EmptyLogTag, exception, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a debug-level exception without a tag.</summary>
        public static void Debug(Exception exception, string text = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Debug, EmptyLogTag, exception, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs an info-level exception without a tag.</summary>
        public static void Info(Exception exception, string text = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Info, EmptyLogTag, exception, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a warning-level exception without a tag.</summary>
        public static void Warning(Exception exception, string text = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Warning, EmptyLogTag, exception, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs an error-level exception without a tag.</summary>
        public static void Error(Exception exception, string text = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Error, EmptyLogTag, exception, text, memberName, filePath, lineNumber);
        }

        /// <summary>Logs a fatal-level exception without a tag.</summary>
        public static void Fatal(Exception exception, string text = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Fatal, EmptyLogTag, exception, text, memberName, filePath, lineNumber);
        }

        #endregion

        #region Structured Logging

        /// <summary>
        ///     Logs a trace-level structured message with the specified tag, message template, and arguments.
        ///     The message template is formatted via <c>string.Format</c>.
        /// </summary>
        public static void Trace(LogTag tag, string messageTemplate, object[] args,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Trace, tag, messageTemplate, args, memberName, filePath, lineNumber);
        }

        /// <summary>
        ///     Logs a debug-level structured message with the specified tag, message template, and arguments.
        /// </summary>
        public static void Debug(LogTag tag, string messageTemplate, object[] args,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Debug, tag, messageTemplate, args, memberName, filePath, lineNumber);
        }

        /// <summary>
        ///     Logs an info-level structured message with the specified tag, message template, and arguments.
        /// </summary>
        public static void Info(LogTag tag, string messageTemplate, object[] args,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Info, tag, messageTemplate, args, memberName, filePath, lineNumber);
        }

        /// <summary>
        ///     Logs a warning-level structured message with the specified tag, message template, and arguments.
        /// </summary>
        public static void Warning(LogTag tag, string messageTemplate, object[] args,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Warning, tag, messageTemplate, args, memberName, filePath, lineNumber);
        }

        /// <summary>
        ///     Logs an error-level structured message with the specified tag, message template, and arguments.
        /// </summary>
        public static void Error(LogTag tag, string messageTemplate, object[] args,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Error, tag, messageTemplate, args, memberName, filePath, lineNumber);
        }

        /// <summary>
        ///     Logs a fatal-level structured message with the specified tag, message template, and arguments.
        /// </summary>
        public static void Fatal(LogTag tag, string messageTemplate, object[] args,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0) {
            Log(LogLevelDef.Fatal, tag, messageTemplate, args, memberName, filePath, lineNumber);
        }

        #endregion

        #region Properties

        public static LogLevelDef LogLevel { get; set; } = LogLevelDef.Error;

        /// <summary>
        ///     Returns true if the given log level would produce output.
        ///     Used internally by LogInterpolatedStringHandler for
        ///     compile-time short-circuit evaluation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnabled(LogLevelDef level) {
            return LogLevel <= level;
        }

        public static int LogCapacity { get; set; } = DefaultCapacity;

        /// <summary>
        ///     When true, stack traces are captured for Error/Fatal log entries.
        ///     Defaults to false.
        /// </summary>
        public static bool CaptureStackTrace { get; set; }

        #endregion
    }
}