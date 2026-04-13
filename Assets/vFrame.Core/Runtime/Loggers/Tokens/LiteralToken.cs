// ------------------------------------------------------------
//         File: LiteralToken.cs
//        Brief: A format token that renders a fixed literal
//               string into the log output.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-11
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Text;

namespace vFrame.Core
{
    /// <summary>
    ///     A format token that renders a fixed literal string.
    /// </summary>
    public class LiteralToken : IToken
    {
        private readonly string _text;

        /// <summary>
        ///     Creates a new literal token with the specified text.
        /// </summary>
        /// <param name="text">The fixed text to render.</param>
        public LiteralToken(string text) {
            _text = text;
        }

        /// <inheritdoc />
        public void Render(StringBuilder sb, Logger.LogContext context) {
            sb.Append(_text);
        }
    }
}