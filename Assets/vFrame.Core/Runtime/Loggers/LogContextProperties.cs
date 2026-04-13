// ------------------------------------------------------------
//         File: LogContextProperties.cs
//        Brief: Async-local property bag for enriching log
//               contexts with scoped key-value pairs.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-11
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;
using System.Threading;

namespace vFrame.Core
{
    /// <summary>
    ///     Manages async-local properties that can be attached to log contexts.
    ///     Properties are automatically propagated across async/await boundaries.
    /// </summary>
    public static class LogContextProperties
    {
        private static readonly AsyncLocal<Dictionary<string, object>> _properties =
            new AsyncLocal<Dictionary<string, object>>();

        private static readonly IReadOnlyDictionary<string, object> EmptyProperties =
            new Dictionary<string, object>();

        /// <summary>
        ///     Pushes a property value onto the current async-local context.
        ///     If the key already exists, its value is overwritten.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        public static void PushProperty(string key, object value) {
            if (_properties.Value == null) {
                _properties.Value = new Dictionary<string, object>();
            }

            _properties.Value[key] = value;
        }

        /// <summary>
        ///     Removes a property from the current async-local context.
        /// </summary>
        /// <param name="key">The property key to remove.</param>
        public static void PopProperty(string key) {
            _properties.Value?.Remove(key);
        }

        /// <summary>
        ///     Returns a read-only snapshot of the current async-local properties.
        ///     Returns an empty dictionary if no properties are set.
        /// </summary>
        /// <returns>A read-only dictionary of current properties.</returns>
        public static IReadOnlyDictionary<string, object> GetCurrentProperties() {
            var props = _properties.Value;
            if (props == null || props.Count == 0) {
                return EmptyProperties;
            }

            return new Dictionary<string, object>(props);
        }
    }
}