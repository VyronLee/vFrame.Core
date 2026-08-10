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

namespace vFrame.Core
{
    /// <summary>
    ///     A format token that renders the log tag ({tag}), omitting the untagged sentinel.
    /// </summary>
    public class TagToken : IToken
    {
        /// <inheritdoc />
        public void Render(StringBuilder sb, Logger.LogContext context) {
            var tag = context.Tag.ToString();
            if (tag == "__EMPTY__") {
                return;
            }

            sb.Append(tag);
        }
    }
}