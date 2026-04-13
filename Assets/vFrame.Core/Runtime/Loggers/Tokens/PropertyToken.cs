// ------------------------------------------------------------
//         File: PropertyToken.cs
//        Brief: Format token that renders a named property from
//               the current log context properties.
//               Syntax: {property:Key}
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
    ///     A format token that renders a named property from the current
    ///     async-local log context properties. Syntax: {property:Key}
    /// </summary>
    public class PropertyToken : IToken
    {
        private readonly string _key;

        /// <summary>
        ///     Creates a new property token for the specified property key.
        /// </summary>
        /// <param name="key">The property key to look up in context properties.</param>
        public PropertyToken(string key) {
            _key = key;
        }

        /// <inheritdoc />
        public void Render(StringBuilder sb, Logger.LogContext context) {
            var properties = LogContextProperties.GetCurrentProperties();
            if (properties != null && properties.TryGetValue(_key, out var value)) {
                sb.Append(value?.ToString() ?? "");
            }
        }
    }
}