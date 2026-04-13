// ------------------------------------------------------------
//         File: TimeToken.cs
//        Brief: Format token that renders the log timestamp.
//               Supports {time} and {time:format} syntax.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-11
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Text;

namespace vFrame.Core
{
    /// <summary>
    ///     A format token that renders the current timestamp.
    ///     Supports {time} (default format) and {time:format} (custom format string).
    /// </summary>
    public class TimeToken : IToken
    {
        private static readonly string DefaultFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private readonly string _format;

        /// <summary>
        ///     Creates a new time token with the specified format string.
        /// </summary>
        /// <param name="format">The DateTime format string. Uses default format if null or empty.</param>
        public TimeToken(string format) {
            _format = string.IsNullOrEmpty(format) ? DefaultFormat : format;
        }

        /// <inheritdoc />
        public void Render(StringBuilder sb, Logger.LogContext context) {
            sb.Append(DateTime.Now.ToString(_format));
        }
    }
}