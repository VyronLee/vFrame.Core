// ------------------------------------------------------------
//         File: LineToken.cs
//        Brief: Format token that renders the source line number.
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
    ///     A format token that renders the source line number ({line}).
    /// </summary>
    public class LineToken : IToken
    {
        /// <inheritdoc />
        public void Render(StringBuilder sb, Logger.LogContext context) {
            sb.Append(context.LineNumber);
        }
    }
}