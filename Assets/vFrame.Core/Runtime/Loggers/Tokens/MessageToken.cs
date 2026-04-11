// ------------------------------------------------------------
//         File: MessageToken.cs
//        Brief: Format token that renders the log message content.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-11
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Text;
using vFrame.Core;

namespace vFrame.Core
{
    /// <summary>
    /// A format token that renders the formatted log message content ({message}).
    /// </summary>
    public class MessageToken : IToken
    {
        /// <inheritdoc/>
        public void Render(StringBuilder sb, Logger.LogContext context) {
            sb.Append(context.Content);
        }
    }
}
