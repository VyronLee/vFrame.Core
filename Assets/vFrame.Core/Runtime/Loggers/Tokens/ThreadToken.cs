// ------------------------------------------------------------
//         File: ThreadToken.cs
//        Brief: Format token that renders the current managed
//               thread ID in [T:id] format.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-11
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Text;
using vFrame.Core;

namespace vFrame.Core
{
    /// <summary>
    /// A format token that renders the current managed thread ID ({thread}).
    /// Output format: [T:id]
    /// </summary>
    public class ThreadToken : IToken
    {
        /// <inheritdoc/>
        public void Render(StringBuilder sb, Logger.LogContext context) {
            sb.Append("[T:");
            sb.Append(Environment.CurrentManagedThreadId);
            sb.Append(']');
        }
    }
}
