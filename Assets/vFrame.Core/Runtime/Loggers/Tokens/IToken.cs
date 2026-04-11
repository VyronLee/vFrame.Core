// ------------------------------------------------------------
//         File: IToken.cs
//        Brief: Interface for log format tokens used by
//               the template-based log formatter.
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
    /// Interface for a single format token in a log template.
    /// </summary>
    public interface IToken
    {
        /// <summary>
        /// Renders this token into the given string builder using the provided log context.
        /// </summary>
        /// <param name="sb">The string builder to append to.</param>
        /// <param name="context">The log context containing values for rendering.</param>
        void Render(StringBuilder sb, Logger.LogContext context);
    }
}
