// ------------------------------------------------------------
//         File: LogInterpolatedStringHandler.cs
//        Brief: Zero-GC interpolated string handler for Logger.
//
//               When the log level is disabled, the handler
//               constructor sets handlerIsValid=false, and the
//               compiler skips ALL AppendFormatted calls —
//               zero allocations, zero method invocations.
//
//               When enabled, the handler builds the formatted
//               string via StringBuilderPool (one allocation
//               for the final string, reused StringBuilder).
//
//      Created: 2026-04-12
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace vFrame.Core
{
    /// <summary>
    ///     Compiler-generated interpolated string handler for log messages.
    ///     Short-circuits at construction time when the log level is disabled.
    /// </summary>
    [InterpolatedStringHandler]
    public ref struct LogInterpolatedStringHandler
    {
        private StringBuilder _builder;
        private readonly bool _isValid;

        /// <summary>
        ///     Constructor for <see cref="Logger" /> static methods.
        ///     Checks global log level — when disabled, all AppendFormatted
        ///     calls are skipped by the compiler (zero GC).
        /// </summary>
        public LogInterpolatedStringHandler(
            int literalLength, int formattedCount,
            LogLevelDef level,
            out bool handlerIsValid) {
            handlerIsValid = Logger.IsEnabled(level);
            _isValid = handlerIsValid;
            _builder = handlerIsValid
                ? StringBuilderPool.Shared.Get()
                : null;
        }

        /// <summary>
        ///     Constructor for <see cref="ILogger" /> extension methods.
        ///     Checks both category minimum level and global level.
        /// </summary>
        public LogInterpolatedStringHandler(
            int literalLength, int formattedCount,
            ILogger logger, LogLevelDef level,
            out bool handlerIsValid) {
            handlerIsValid = logger != null && logger.IsEnabled(level);
            _isValid = handlerIsValid;
            _builder = handlerIsValid
                ? StringBuilderPool.Shared.Get()
                : null;
        }

        public void AppendLiteral(string s) {
            if (_isValid) {
                _builder.Append(s);
            }
        }

        public void AppendFormatted<T>(T value) {
            if (!_isValid) {
                return;
            }

            _builder.Append(value?.ToString() ?? "(null)");
        }

        public void AppendFormatted<T>(T value, int alignment) {
            if (!_isValid) {
                return;
            }

            AppendWithAlignment(value?.ToString() ?? "(null)", alignment);
        }

        public void AppendFormatted<T>(T value, string format) {
            if (!_isValid) {
                return;
            }

            if (value is IFormattable formattable) {
                _builder.Append(formattable.ToString(format, null));
            }
            else {
                _builder.Append(value?.ToString() ?? "(null)");
            }
        }

        public void AppendFormatted<T>(T value, int alignment, string format) {
            if (!_isValid) {
                return;
            }

            string s;
            if (value is IFormattable formattable) {
                s = formattable.ToString(format, null);
            }
            else {
                s = value?.ToString() ?? "(null)";
            }

            AppendWithAlignment(s, alignment);
        }

        public void AppendFormatted(ReadOnlySpan<char> value) {
            if (_isValid) {
                _builder.Append(value);
            }
        }

        public void AppendFormatted(ReadOnlySpan<char> value, int alignment, string format) {
            if (!_isValid) {
                return;
            }

            if (alignment == 0) {
                _builder.Append(value);
                return;
            }

            AppendWithAlignment(value.ToString(), alignment);
        }

        /// <summary>
        ///     Returns the formatted log text, or null if the handler was disabled.
        ///     The internal StringBuilder is returned to the pool.
        /// </summary>
        public string GetFormattedText() {
            if (!_isValid) {
                return null;
            }

            var text = _builder.ToString();
            StringBuilderPool.Shared.Return(_builder);
            _builder = null;
            return text;
        }

        private void AppendWithAlignment(string s, int alignment) {
            if (alignment == 0) {
                _builder.Append(s);
                return;
            }

            var padCount = Math.Max(0, Math.Abs(alignment) - s.Length);
            if (alignment < 0) {
                _builder.Append(s);
                if (padCount > 0) {
                    _builder.Append(' ', padCount);
                }
            }
            else {
                if (padCount > 0) {
                    _builder.Append(' ', padCount);
                }

                _builder.Append(s);
            }
        }
    }
}