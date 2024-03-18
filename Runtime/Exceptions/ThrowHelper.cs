// ------------------------------------------------------------
//         File: ThrowHelper.cs
//        Brief: ThrowHelper.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2023-12-25 22:58
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Collections.Generic;
using System.Linq;

namespace vFrame.Core.Exceptions
{
    internal static class ThrowHelper
    {
        public static void ThrowArgumentException(string message) {
            throw new vFrameArgumentException(message);
        }

        public static void ThrowIfNull(object param, string variable) {
            if (null != param) {
                return;
            }
            throw new vFrameArgumentNullException(variable);
        }

        public static void ThrowIfNullOrEmpty<T>(IEnumerable<T> param, string variable) {
            if (null != param && param.Any()) {
                return;
            }
            throw new vFrameArgumentException($"Variable ${variable} cannot be null or empty!");
        }

        public static void ThrowUnsupportedEnum<T>(T value) {
            throw new vFrameUnsupportedEnumException($"Unsupported enum value: {value}!");
        }

        public static void ThrowUndesiredException(string message) {
            throw new vFrameException(message);
        }

        public static string Variables(params string[] args) {
            if (null == args || args.Length <= 0) {
                return "";
            }
            return string.Join(".", args);
        }
    }
}