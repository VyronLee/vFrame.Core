// ------------------------------------------------------------
//         File: TagToken.cs
//        Brief: Format token that renders the log tag.
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
    /// A format token that renders the log tag ({tag}).
    /// </summary>
    public class TagToken : IToken
    {
        /// <inheritdoc/>
        public void Render(StringBuilder sb, Logger.LogContext context) {
            sb.Append(context.Tag.ToString());
        }
    }
}
