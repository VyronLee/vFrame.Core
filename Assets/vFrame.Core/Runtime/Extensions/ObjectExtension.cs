// ------------------------------------------------------------
//         File: ObjectExtension.cs
//        Brief: Object extension methods providing deep copy and deep compare
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-09 16:56
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Generic;

namespace vFrame.Core
{
    public static class ObjectExtensions
    {
        /// <summary>
        ///     Determines whether the type is a primitive type (including string).
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><c>true</c> if the type is a primitive type; otherwise, <c>false</c>.</returns>
        public static bool IsPrimitive(this Type type) {
            if (type == typeof(string)) {
                return true;
            }

            return type.IsValueType & type.IsPrimitive;
        }

        /// <summary>
        ///     Creates a deep copy of the object.
        /// </summary>
        /// <param name="original">The original object.</param>
        /// <typeparam name="T">The object type.</typeparam>
        /// <returns>A new deep-copied object.</returns>
        public static T DeepCopy<T>(this T original) {
            return (T)ObjectCopy.DeepCopy(original);
        }

        /// <summary>
        ///     Deep copies values from the original object into the target object.
        /// </summary>
        /// <param name="original">The source object.</param>
        /// <param name="target">The target object to copy into.</param>
        /// <typeparam name="T">The object type.</typeparam>
        /// <returns>The target object after copying.</returns>
        public static T DeepCopyFrom<T>(this T original, T target) {
            return (T)ObjectCopy.DeepCopyFrom(original, target);
        }

        /// <summary>
        ///     Deeply compares two objects for equality.
        /// </summary>
        /// <param name="original">The first object.</param>
        /// <param name="target">The second object.</param>
        /// <typeparam name="T">The object type.</typeparam>
        /// <returns><c>true</c> if the objects are deeply equal; otherwise, <c>false</c>.</returns>
        public static bool DeepCompare<T>(this T original, T target) {
            var comparer = new Comparer<T>();
            return comparer.Compare(original, target);
        }

        /// <summary>
        ///     Deeply compares two objects for equality using a custom comparer factory.
        /// </summary>
        /// <param name="original">The first object.</param>
        /// <param name="target">The second object.</param>
        /// <param name="factory">The custom comparers factory.</param>
        /// <typeparam name="T">The object type.</typeparam>
        /// <returns><c>true</c> if the objects are deeply equal; otherwise, <c>false</c>.</returns>
        public static bool DeepCompare<T>(this T original, T target, IComparersFactory factory) {
            var comparer = new Comparer<T>(factory: factory);
            return comparer.Compare(original, target);
        }

        /// <summary>
        ///     Deeply compares two objects and outputs the list of differences.
        /// </summary>
        /// <param name="original">The first object.</param>
        /// <param name="target">The second object.</param>
        /// <param name="differences">The differences found during comparison.</param>
        /// <typeparam name="T">The object type.</typeparam>
        /// <returns><c>true</c> if the objects are deeply equal; otherwise, <c>false</c>.</returns>
        public static bool DeepCompare<T>(this T original, T target, out IEnumerable<Difference> differences) {
            var comparer = new Comparer<T>();
            return comparer.Compare(original, target, out differences);
        }

        /// <summary>
        ///     Deeply compares two objects using a custom comparer factory and outputs the list of differences.
        /// </summary>
        /// <param name="original">The first object.</param>
        /// <param name="target">The second object.</param>
        /// <param name="factory">The custom comparers factory.</param>
        /// <param name="differences">The differences found during comparison.</param>
        /// <typeparam name="T">The object type.</typeparam>
        /// <returns><c>true</c> if the objects are deeply equal; otherwise, <c>false</c>.</returns>
        public static bool DeepCompare<T>(this T original, T target, IComparersFactory factory,
            out IEnumerable<Difference> differences) {
            var comparer = new Comparer<T>(factory: factory);
            return comparer.Compare(original, target, out differences);
        }
    }
}