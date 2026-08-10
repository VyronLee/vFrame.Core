// ------------------------------------------------------------
//         File: LogTag.cs
//        Brief: Value type representing a named log tag used
//               to categorize and filter log output.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 19:50:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("vFrame.Core.Unity")]

namespace vFrame.Core
{
    public struct LogTag : IEquatable<LogTag>
    {
        private readonly object _identity;
        private readonly string _name;

        /// <summary>
        ///     Creates a new log tag with the specified name.
        /// </summary>
        /// <param name="name">The tag name. Defaults to "undefined".</param>
        public LogTag(string name = "undefined") {
            _identity = null;
            _name = name;
        }

        internal LogTag(string name, object identity) {
            _identity = identity;
            _name = name;
        }

        internal bool HasSameIdentity(LogTag other) {
            return _identity != null && ReferenceEquals(_identity, other._identity);
        }

        /// <summary>
        ///     Returns the tag name.
        /// </summary>
        /// <returns>The tag name string.</returns>
        public override string ToString() {
            return _name;
        }

        /// <summary>
        ///     Determines whether this tag is equal to another log tag by name comparison.
        /// </summary>
        /// <param name="other">The other log tag to compare.</param>
        /// <returns><c>true</c> if both tags have the same name; otherwise, <c>false</c>.</returns>
        public bool Equals(LogTag other) {
            return _name == other._name;
        }

        /// <summary>
        ///     Determines whether this tag is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><c>true</c> if the object is a <see cref="LogTag" /> with the same name; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj) {
            return obj is LogTag other && Equals(other);
        }

        /// <summary>
        ///     Returns a hash code based on the tag name.
        /// </summary>
        /// <returns>The hash code for this tag.</returns>
        public override int GetHashCode() {
            return _name != null ? _name.GetHashCode() : 0;
        }
    }
}