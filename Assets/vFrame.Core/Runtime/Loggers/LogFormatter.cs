// ------------------------------------------------------------
//         File: LogFormatter.cs
//        Brief: Template-based log formatter that parses format
//               strings with {token} and {token:format} syntax
//               into renderable tokens.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-11
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;
using System.Text;
using vFrame.Core;

namespace vFrame.Core
{
    /// <summary>
    /// Template-based log formatter that parses format strings containing
    /// <c>{token}</c> and <c>{token:format}</c> placeholders into renderable tokens.
    /// </summary>
    public class LogFormatter
    {
        private readonly List<IToken> _tokens = new List<IToken>();

        /// <summary>
        /// Creates a new log formatter by parsing the specified template string.
        /// </summary>
        /// <param name="template">The format template (e.g., "[{time}] [{level:u3}] {tag}: {message}").</param>
        public LogFormatter(string template) {
            Parse(template ?? "");
        }

        /// <summary>
        /// Formats the given log context using the parsed template tokens.
        /// </summary>
        /// <param name="context">The log context to format.</param>
        /// <returns>The formatted log string.</returns>
        public string Format(Logger.LogContext context) {
            var sb = StringBuilderPool.Shared.Get();
            try {
                foreach (var token in _tokens) {
                    token.Render(sb, context);
                }
                return sb.ToString();
            }
            finally {
                StringBuilderPool.Shared.Return(sb);
            }
        }

        private void Parse(string template) {
            var i = 0;
            var literalStart = 0;

            while (i < template.Length) {
                if (template[i] == '{') {
                    // Flush preceding literal
                    if (i > literalStart) {
                        _tokens.Add(new LiteralToken(template.Substring(literalStart, i - literalStart)));
                    }

                    // Find closing brace
                    var closeIdx = template.IndexOf('}', i + 1);
                    if (closeIdx < 0) {
                        // No closing brace found; treat the rest as literal
                        _tokens.Add(new LiteralToken(template.Substring(i)));
                        return;
                    }

                    var inner = template.Substring(i + 1, closeIdx - i - 1);
                    var colonIdx = inner.IndexOf(':');

                    string tokenName;
                    string format;

                    if (colonIdx >= 0) {
                        tokenName = inner.Substring(0, colonIdx);
                        format = inner.Substring(colonIdx + 1);
                    }
                    else {
                        tokenName = inner;
                        format = null;
                    }

                    _tokens.Add(CreateToken(tokenName, format));
                    i = closeIdx + 1;
                    literalStart = i;
                }
                else {
                    i++;
                }
            }

            // Flush trailing literal
            if (literalStart < template.Length) {
                _tokens.Add(new LiteralToken(template.Substring(literalStart)));
            }
        }

        private static IToken CreateToken(string tokenName, string format) {
            switch (tokenName) {
                case "time":
                    return new TimeToken(format);
                case "level":
                    return new LevelToken(format);
                case "tag":
                    return new TagToken();
                case "message":
                    return new MessageToken();
                case "exception":
                    return new ExceptionToken();
                case "thread":
                    return new ThreadToken();
                case "line":
                    return new LineToken();
                case "property":
                    return new PropertyToken(format);
                default:
                    // Unknown token — render as literal
                    var sb = StringBuilderPool.Shared.Get();
                    try {
                        sb.Append('{');
                        sb.Append(tokenName);
                        if (format != null) {
                            sb.Append(':');
                            sb.Append(format);
                        }
                        sb.Append('}');
                        return new LiteralToken(sb.ToString());
                    }
                    finally {
                        StringBuilderPool.Shared.Return(sb);
                    }
            }
        }
    }
}
