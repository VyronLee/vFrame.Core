// ------------------------------------------------------------
//         File: JsonLogSink.cs
//        Brief: Structured log sink that outputs JSON formatted
//               log entries to an internal queue.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-11
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace vFrame.Core
{
    /// <summary>
    ///     A structured log sink that formats log contexts as JSON strings
    ///     and stores them in a thread-safe queue for retrieval.
    /// </summary>
    public class JsonLogSink : Logger.IStructuredLogSink
    {
        private readonly ConcurrentQueue<string> _outputQueue = new ConcurrentQueue<string>();

        /// <summary>
        ///     Called when a new log context is received. Formats the context as JSON
        ///     and enqueues it.
        /// </summary>
        /// <param name="context">The log context to format.</param>
        public void OnLogReceived(Logger.LogContext context) {
            var json = FormatAsJson(context);
            _outputQueue.Enqueue(json);
        }

        /// <summary>
        ///     Returns all JSON strings that have been output by this sink.
        /// </summary>
        /// <returns>A list of JSON formatted log entries.</returns>
        public List<string> GetOutput() {
            return new List<string>(_outputQueue);
        }

        /// <summary>
        ///     Clears all output from the queue.
        /// </summary>
        public void Clear() {
            while (_outputQueue.TryDequeue(out _)) { }
        }

        private static string FormatAsJson(Logger.LogContext context) {
            var sb = StringBuilderPool.Shared.Get();
            try {
                sb.Append('{');

                AppendKeyValue(sb, "level", context.Level.ToString());
                sb.Append(',');
                AppendKeyValue(sb, "tag", context.Tag.ToString());
                sb.Append(',');
                AppendKeyValue(sb, "memberName", context.MemberName);
                sb.Append(',');
                AppendKeyValue(sb, "lineNumber", context.LineNumber.ToString());
                sb.Append(',');
                AppendKeyValue(sb, "filePath", context.FilePath);
                sb.Append(',');
                AppendKeyValue(sb, "messageTemplate", context.MessageTemplate);
                sb.Append(',');

                sb.Append("\"args\":");
                if (context.Args != null && context.Args.Length > 0) {
                    sb.Append('[');
                    for (var i = 0; i < context.Args.Length; i++) {
                        if (i > 0) {
                            sb.Append(',');
                        }

                        var arg = context.Args[i];
                        if (arg == null) {
                            sb.Append("null");
                        }
                        else if (arg is string s) {
                            sb.Append('"');
                            sb.Append(EscapeJson(s));
                            sb.Append('"');
                        }
                        else {
                            sb.Append(arg);
                        }
                    }

                    sb.Append(']');
                }
                else {
                    sb.Append("null");
                }

                sb.Append(',');

                AppendKeyValue(sb, "content", context.Content);
                sb.Append(',');

                sb.Append("\"exception\":");
                if (context.Exception != null) {
                    sb.Append('"');
                    sb.Append(EscapeJson(context.Exception.ToString()));
                    sb.Append('"');
                }
                else {
                    sb.Append("null");
                }

                sb.Append(',');

                AppendKeyValue(sb, "time", DateTime.Now.ToString("O"));

                sb.Append('}');
                return sb.ToString();
            }
            finally {
                StringBuilderPool.Shared.Return(sb);
            }
        }

        private static void AppendKeyValue(StringBuilder sb, string key, string value) {
            sb.Append('"');
            sb.Append(key);
            sb.Append("\":\"");
            sb.Append(EscapeJson(value ?? ""));
            sb.Append('"');
        }

        private static string EscapeJson(string s) {
            if (string.IsNullOrEmpty(s)) {
                return s;
            }

            var sb = StringBuilderPool.Shared.Get();
            try {
                foreach (var c in s) {
                    switch (c) {
                        case '"':
                            sb.Append("\\\"");
                            break;
                        case '\\':
                            sb.Append("\\\\");
                            break;
                        case '\b':
                            sb.Append("\\b");
                            break;
                        case '\f':
                            sb.Append("\\f");
                            break;
                        case '\n':
                            sb.Append("\\n");
                            break;
                        case '\r':
                            sb.Append("\\r");
                            break;
                        case '\t':
                            sb.Append("\\t");
                            break;
                        default:
                            if (c < ' ') {
                                sb.Append("\\u");
                                sb.Append(((int)c).ToString("x4"));
                            }
                            else {
                                sb.Append(c);
                            }

                            break;
                    }
                }

                return sb.ToString();
            }
            finally {
                StringBuilderPool.Shared.Return(sb);
            }
        }
    }
}