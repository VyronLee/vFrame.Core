// ------------------------------------------------------------
//         File: LoggerExtensions.cs
//        Brief: InterpolatedStringHandler extension methods
//               for ILogger — enables zero-GC logging on
//               category-based logger instances.
//
//      Created: 2026-04-12
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Runtime.CompilerServices;

namespace vFrame.Core
{
    /// <summary>
    ///     Zero-GC interpolated string extensions for <see cref="ILogger" />.
    ///     The handler constructor checks both category and global levels,
    ///     so disabled calls produce zero allocations.
    /// </summary>
    public static class LoggerExtensions
    {
        public static void Trace(this ILogger logger,
            LogLevelDef level,
            [InterpolatedStringHandlerArgument("logger", "level")]
            LogInterpolatedStringHandler handler) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            logger.Trace(text);
        }

        public static void Debug(this ILogger logger,
            LogLevelDef level,
            [InterpolatedStringHandlerArgument("logger", "level")]
            LogInterpolatedStringHandler handler) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            logger.Debug(text);
        }

        public static void Info(this ILogger logger,
            LogLevelDef level,
            [InterpolatedStringHandlerArgument("logger", "level")]
            LogInterpolatedStringHandler handler) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            logger.Info(text);
        }

        public static void Warning(this ILogger logger,
            LogLevelDef level,
            [InterpolatedStringHandlerArgument("logger", "level")]
            LogInterpolatedStringHandler handler) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            logger.Warning(text);
        }

        public static void Error(this ILogger logger,
            LogLevelDef level,
            [InterpolatedStringHandlerArgument("logger", "level")]
            LogInterpolatedStringHandler handler) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            logger.Error(text);
        }

        public static void Fatal(this ILogger logger,
            LogLevelDef level,
            [InterpolatedStringHandlerArgument("logger", "level")]
            LogInterpolatedStringHandler handler) {
            var text = handler.GetFormattedText();
            if (text == null) {
                return;
            }

            logger.Fatal(text);
        }
    }
}