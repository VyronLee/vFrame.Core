// Decompiled with JetBrains decompiler
// Type: UnityEngine.StackTraceUtility
// Assembly: UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null

using System;
using System.Diagnostics;
using System.Security;
using System.Text;

namespace vFrame.Core.Loggers
{
    public static class StackTraceUtility
    {
        private static string projectFolder = string.Empty;

        internal static void SetProjectFolder(string folder) {
            projectFolder = folder;
        }

        [SecuritySafeCritical]
        public static string ExtractStackTrace() {
            return ExtractFormattedStackTrace(new StackTrace(1, true)).ToString();
        }

        private static bool IsSystemStacktraceType(object name) {
            var str = (string) name;
            if (!str.StartsWith("UnityEditor.") && !str.StartsWith("UnityEngine.") && !str.StartsWith("System.") &&
                !str.StartsWith("UnityScript.Lang.") && !str.StartsWith("Boo.Lang."))
                return str.StartsWith("UnityEngine.SetupCoroutine");
            return true;
        }

        public static string ExtractStringFromException(object exception) {
            var message = string.Empty;
            var stackTrace = string.Empty;
            ExtractStringFromExceptionInternal(exception, out message, out stackTrace);
            return message + "\n" + stackTrace;
        }

        [SecuritySafeCritical]
        internal static void ExtractStringFromExceptionInternal(object exceptiono, out string message,
            out string stackTrace) {
            if (exceptiono == null)
                throw new ArgumentException("ExtractStringFromExceptionInternal called with null exception");
            var exception = exceptiono as Exception;
            if (exception == null)
                throw new ArgumentException(
                    "ExtractStringFromExceptionInternal called with an exceptoin that was not of type System.Exception");
            var stringBuilder = new StringBuilder(exception.StackTrace != null ? exception.StackTrace.Length * 2 : 512);
            message = string.Empty;
            var str1 = string.Empty;
            for (; exception != null; exception = exception.InnerException) {
                str1 = str1.Length != 0 ? exception.StackTrace + "\n" + str1 : exception.StackTrace;
                var str2 = exception.GetType().Name;
                var str3 = string.Empty;
                if (exception.Message != null)
                    str3 = exception.Message;
                if (str3.Trim().Length != 0)
                    str2 = str2 + ": " + str3;
                message = str2;
                if (exception.InnerException != null)
                    str1 = "Rethrow as " + str2 + "\n" + str1;
            }

            stringBuilder.Append(str1 + "\n");
            var stackTrace1 = new StackTrace(1, true);
            stringBuilder.Append(ExtractFormattedStackTrace(stackTrace1));
            stackTrace = stringBuilder.ToString();
        }

        internal static string PostprocessStacktrace(string oldString, bool stripEngineInternalInformation) {
            if (oldString == null)
                return string.Empty;
            var strArray = oldString.Split('\n');
            var stringBuilder = new StringBuilder(oldString.Length);
            for (var index = 0; index < strArray.Length; ++index)
                strArray[index] = strArray[index].Trim();
            for (var index = 0; index < strArray.Length; ++index) {
                var str1 = strArray[index];
                if (str1.Length != 0 && (int) str1[0] != 10 && !str1.StartsWith("in (unmanaged)")) {
                    if (!stripEngineInternalInformation ||
                        !str1.StartsWith("UnityEditor.EditorGUIUtility:RenderGameViewCameras")) {
                        if (stripEngineInternalInformation && index < strArray.Length - 1 &&
                            IsSystemStacktraceType((object) str1)) {
                            if (!IsSystemStacktraceType((object) strArray[index + 1])) {
                                var length = str1.IndexOf(" (at");
                                if (length != -1)
                                    str1 = str1.Substring(0, length);
                            }
                            else {
                                continue;
                            }
                        }

                        if (str1.IndexOf("(wrapper managed-to-native)") == -1 &&
                            str1.IndexOf("(wrapper delegate-invoke)") == -1 &&
                            str1.IndexOf("at <0x00000> <unknown method>") == -1 &&
                            (!stripEngineInternalInformation || !str1.StartsWith("[") || !str1.EndsWith("]"))) {
                            if (str1.StartsWith("at "))
                                str1 = str1.Remove(0, 3);
                            var startIndex1 = str1.IndexOf("[0x");
                            var num = -1;
                            if (startIndex1 != -1)
                                num = str1.IndexOf("]", startIndex1);
                            if (startIndex1 != -1 && num > startIndex1)
                                str1 = str1.Remove(startIndex1, num - startIndex1 + 1);
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

        [SecuritySafeCritical]
        internal static string ExtractFormattedStackTrace(StackTrace stackTrace) {
            var stringBuilder = new StringBuilder((int) byte.MaxValue);
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
                            if (!flag)
                                stringBuilder.Append(", ");
                            else
                                flag = false;
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
                            if (str2.StartsWith(projectFolder))
                                str2 = str2.Substring(projectFolder.Length, str2.Length - projectFolder.Length);
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