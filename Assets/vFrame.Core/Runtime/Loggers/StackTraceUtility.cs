// ------------------------------------------------------------
//         File: StackTraceUtility.cs
//        Brief: Utility for extracting and formatting stack
//               traces, adapted from Unity's stack trace API.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2018-10-20 18:09:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Diagnostics;
using System.Security;
using System.Text;

namespace vFrame.Core
{
    public static class StackTraceUtility
    {
        private static string projectFolder = string.Empty;

        /// <summary>
        /// Sets the project root folder used to strip absolute paths from stack traces.
        /// </summary>
        /// <param name="folder">The absolute path of the project root folder.</param>
        internal static void SetProjectFolder(string folder) {
            projectFolder = folder;
        }

        /// <summary>
        /// Extracts a formatted stack trace from the current call site.
        /// </summary>
        /// <returns>A formatted string representation of the stack trace.</returns>
        [SecuritySafeCritical]
        public static string ExtractStackTrace() {
            return ExtractFormattedStackTrace(new StackTrace(1, true));
        }

        /// <summary>
        /// Determines whether the given type name belongs to a system or engine namespace.
        /// </summary>
        /// <param name="name">The type name to check.</param>
        /// <returns><c>true</c> if the name is a system stack trace type; otherwise, <c>false</c>.</returns>
        private static bool IsSystemStacktraceType(object name) {
            var str = (string)name;
            if (!str.StartsWith("UnityEditor.") && !str.StartsWith("UnityEngine.") && !str.StartsWith("System.") &&
                !str.StartsWith("UnityScript.Lang.") && !str.StartsWith("Boo.Lang.")) {
                return str.StartsWith("UnityEngine.SetupCoroutine");
            }
            return true;
        }

        /// <summary>
        /// Extracts the exception message and stack trace from an exception object.
        /// </summary>
        /// <param name="exception">The exception to extract from.</param>
        /// <returns>A combined string of the message and stack trace separated by a newline.</returns>
        public static string ExtractStringFromException(object exception) {
            var message = string.Empty;
            var stackTrace = string.Empty;
            ExtractStringFromExceptionInternal(exception, out message, out stackTrace);
            return message + "\n" + stackTrace;
        }

        /// <summary>
        /// Internal method that extracts message and stack trace from an exception,
        /// walking the inner exception chain.
        /// </summary>
        /// <param name="exceptiono">The exception object to extract from.</param>
        /// <param name="message">The combined exception message.</param>
        /// <param name="stackTrace">The combined stack trace string.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="exceptiono"/> is null or not an <see cref="Exception"/>.</exception>
        [SecuritySafeCritical]
        internal static void ExtractStringFromExceptionInternal(object exceptiono, out string message,
            out string stackTrace) {
            if (exceptiono == null) {
                throw new ArgumentException("ExtractStringFromExceptionInternal called with null exception");
            }
            var exception = exceptiono as Exception;
            if (exception == null) {
                throw new ArgumentException(
                    "ExtractStringFromExceptionInternal called with an exceptoin that was not of type System.Exception");
            }
            var stringBuilder = new StringBuilder(exception.StackTrace != null ? exception.StackTrace.Length * 2 : 512);
            message = string.Empty;
            var str1 = string.Empty;
            for (; exception != null; exception = exception.InnerException) {
                str1 = str1.Length != 0 ? exception.StackTrace + "\n" + str1 : exception.StackTrace;
                var str2 = exception.GetType().Name;
                var str3 = string.Empty;
                if (exception.Message != null) {
                    str3 = exception.Message;
                }
                if (str3.Trim().Length != 0) {
                    str2 = str2 + ": " + str3;
                }
                message = str2;
                if (exception.InnerException != null) {
                    str1 = "Rethrow as " + str2 + "\n" + str1;
                }
            }

            stringBuilder.Append(str1 + "\n");
            var stackTrace1 = new StackTrace(1, true);
            stringBuilder.Append(ExtractFormattedStackTrace(stackTrace1));
            stackTrace = stringBuilder.ToString();
        }

        /// <summary>
        /// Post-processes a raw stack trace string, stripping engine-internal frames
        /// and normalizing path separators.
        /// </summary>
        /// <param name="oldString">The raw stack trace string.</param>
        /// <param name="stripEngineInternalInformation">Whether to remove engine-internal frames.</param>
        /// <returns>The post-processed stack trace string.</returns>
        internal static string PostprocessStacktrace(string oldString, bool stripEngineInternalInformation) {
            if (oldString == null) {
                return string.Empty;
            }
            var strArray = oldString.Split('\n');
            var stringBuilder = new StringBuilder(oldString.Length);
            for (var index = 0; index < strArray.Length; ++index) {
                strArray[index] = strArray[index].Trim();
            }
            for (var index = 0; index < strArray.Length; ++index) {
                var str1 = strArray[index];
                if (str1.Length != 0 && str1[0] != 10 && !str1.StartsWith("in (unmanaged)")) {
                    if (!stripEngineInternalInformation ||
                        !str1.StartsWith("UnityEditor.EditorGUIUtility:RenderGameViewCameras")) {
                        if (stripEngineInternalInformation && index < strArray.Length - 1 &&
                            IsSystemStacktraceType(str1)) {
                            if (!IsSystemStacktraceType(strArray[index + 1])) {
                                var length = str1.IndexOf(" (at");
                                if (length != -1) {
                                    str1 = str1.Substring(0, length);
                                }
                            }
                            else {
                                continue;
                            }
                        }

                        if (str1.IndexOf("(wrapper managed-to-native)") == -1 &&
                            str1.IndexOf("(wrapper delegate-invoke)") == -1 &&
                            str1.IndexOf("at <0x00000> <unknown method>") == -1 &&
                            (!stripEngineInternalInformation || !str1.StartsWith("[") || !str1.EndsWith("]"))) {
                            if (str1.StartsWith("at ")) {
                                str1 = str1.Remove(0, 3);
                            }
                            var startIndex1 = str1.IndexOf("[0x");
                            var num = -1;
                            if (startIndex1 != -1) {
                                num = str1.IndexOf("]", startIndex1);
                            }
                            if (startIndex1 != -1 && num > startIndex1) {
                                str1 = str1.Remove(startIndex1, num - startIndex1 + 1);
                            }
                            var str2 = str1.Replace("  in <filename unknown>:0", string.Empty)
                                .Replace(projectFolder, string.Empty).Replace('\\', '/');
                            var startIndex2 = str2.LastIndexOf("  in ");
                            if (startIndex2 != -1) {
                                var str3 = str2.Remove(startIndex2, 5).Insert(startIndex2, " (at ");
                                str2 = str3.Insert(str3.Length, ")");
                            }

                            stringBuilder.Append(str2 + "\n");
                        }
                    }
                    else {
                        break;
                    }
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Formats a <see cref="StackTrace"/> into a human-readable string with
        /// namespace, class, method, parameters, and file location.
        /// </summary>
        /// <param name="stackTrace">The stack trace to format.</param>
        /// <returns>A formatted string representation of the stack trace.</returns>
        [SecuritySafeCritical]
        internal static string ExtractFormattedStackTrace(StackTrace stackTrace) {
            var stringBuilder = new StringBuilder(byte.MaxValue);
            for (var index1 = 0; index1 < stackTrace.FrameCount; ++index1) {
                var frame = stackTrace.GetFrame(index1);
                var method = frame.GetMethod();
                if (method != null) {
                    var declaringType = method.DeclaringType;
                    if (declaringType != null) {
                        var str1 = declaringType.Namespace;
                        if (str1 != null && str1.Length != 0) {
                            stringBuilder.Append(str1);
                            stringBuilder.Append(".");
                        }

                        stringBuilder.Append(declaringType.Name);
                        stringBuilder.Append(":");
                        stringBuilder.Append(method.Name);
                        stringBuilder.Append("(");
                        var index2 = 0;
                        var parameters = method.GetParameters();
                        var flag = true;
                        for (; index2 < parameters.Length; ++index2) {
                            if (!flag) {
                                stringBuilder.Append(", ");
                            }
                            else {
                                flag = false;
                            }
                            stringBuilder.Append(parameters[index2].ParameterType.Name);
                        }

                        stringBuilder.Append(")");
                        var str2 = frame.GetFileName();
                        if (str2 != null &&
                            (!(declaringType.Name == "Debug") || !(declaringType.Namespace == "UnityEngine")) &&
                            (!(declaringType.Name == "Logger") || !(declaringType.Namespace == "UnityEngine")) &&
                            (!(declaringType.Name == "DebugLogHandler") ||
                             !(declaringType.Namespace == "UnityEngine")) &&
                            (!(declaringType.Name == "Assert") ||
                             !(declaringType.Namespace == "UnityEngine.Assertions"))) {
                            stringBuilder.Append(" (at ");
                            if (str2.StartsWith(projectFolder)) {
                                str2 = str2.Substring(projectFolder.Length, str2.Length - projectFolder.Length);
                            }
                            stringBuilder.Append(str2);
                            stringBuilder.Append(":");
                            stringBuilder.Append(frame.GetFileLineNumber().ToString());
                            stringBuilder.Append(")");
                        }

                        stringBuilder.Append("\n");
                    }
                }
            }

            return stringBuilder.ToString();
        }
    }
}
