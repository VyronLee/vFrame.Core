// ------------------------------------------------------------
//         File: ComponentExtension.cs
//        Brief: Extension method that deep-copies property and
//               field values from one Component to another of
//               the same type via reflection.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-01-02 21:37:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Reflection;
using UnityEngine;

namespace vFrame.Core.Unity
{
    public static class ComponentExtension
    {
        private const BindingFlags Flags = BindingFlags.Public
                                           | BindingFlags.NonPublic
                                           | BindingFlags.Instance
                                           | BindingFlags.Default
                                           | BindingFlags.DeclaredOnly;

        /// <summary>
        ///     Copies all writable properties and fields from
        ///     <paramref name="other"/> into <paramref name="comp"/>.
        ///     Both components must be of the same type.
        /// </summary>
        /// <param name="comp">The destination component.</param>
        /// <param name="other">The source component to copy from.</param>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>
        ///     The destination component cast to <typeparamref name="T"/>,
        ///     or <c>null</c> if the types do not match.
        /// </returns>
        public static T GetCopyOf<T>(this Component comp, T other) where T : Component {
            var type = comp.GetType();
            if (type != other.GetType()) {
                return null;
            }
            var propertiesInfo = type.GetProperties(Flags);
            foreach (var info in propertiesInfo) {
                if (info.CanWrite) {
                    try {
                        info.SetValue(comp, info.GetValue(other, null), null);
                    }
                    catch {
                        // In case of NotImplementedException being thrown.
                        // For some reason specifying that exception didn't seem to catch it,
                        // so I didn't catch anything specific.
                    }
                }
            }

            var fieldsInfo = type.GetFields(Flags);
            foreach (var info in fieldsInfo) {
                info.SetValue(comp, info.GetValue(other));
            }
            return comp as T;
        }
    }
}
