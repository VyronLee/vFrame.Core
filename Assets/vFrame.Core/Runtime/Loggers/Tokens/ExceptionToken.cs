// ------------------------------------------------------------
//         File: ExceptionToken.cs
//        Brief: Format token that renders exception information
//               from the log context.
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
    /// A format token that renders exception information ({exception}).
    /// Renders nothing if no exception is present.
    /// </summary>
    public class ExceptionToken : IToken
    {
        /// <inheritdoc/>
        public void Render(StringBuilder sb, Logger.LogContext context) {
            if (context.Exception != null) {
                sb.Append(context.Exception.ToString());
            }
        }
    }
}
