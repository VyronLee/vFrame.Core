// ------------------------------------------------------------
//         File: LogScope.cs
//        Brief: Disposable struct that pushes/pops context
//               properties for scoped log enrichment.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-11
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    /// <summary>
    /// A disposable scope that pushes a named property onto the current
    /// async-local log context and pops it on disposal.
    /// </summary>
    public readonly struct LogScope : IDisposable
    {
        private readonly string _key;

        /// <summary>
        /// Pushes a property onto the current log context scope.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        public LogScope(string key, object value) {
            _key = key;
            LogContextProperties.PushProperty(key, value);
        }

        /// <summary>
        /// Pops the property from the current log context scope.
        /// </summary>
        public void Dispose() {
            LogContextProperties.PopProperty(_key);
        }
    }
}
