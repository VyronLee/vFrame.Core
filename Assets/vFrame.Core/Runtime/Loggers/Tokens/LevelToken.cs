// ------------------------------------------------------------
//         File: LevelToken.cs
//        Brief: Format token that renders the log level.
//               Supports {level} (full name) and {level:u3}
//               (3-character uppercase abbreviation).
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
    ///     A format token that renders the log level.
    ///     Supports {level} (full name) and {level:u3} (3-character uppercase abbreviation).
    /// </summary>
    public class LevelToken : IToken
    {
        private readonly string _format;

        /// <summary>
        ///     Creates a new level token with the specified format.
        /// </summary>
        /// <param name="format">Format string. "u3" for 3-character uppercase, null for full name.</param>
        public LevelToken(string format) {
            _format = format;
        }

        /// <inheritdoc />
        public void Render(StringBuilder sb, Logger.LogContext context) {
            if (_format == "u3") {
                sb.Append(GetShortLevel(context.Level));
            }
            else {
                sb.Append(context.Level.ToString());
            }
        }

        private static string GetShortLevel(LogLevelDef level) {
            switch (level) {
                case LogLevelDef.Trace: return "TRC";
                case LogLevelDef.Debug: return "DBG";
                case LogLevelDef.Info: return "INF";
                case LogLevelDef.Warning: return "WRN";
                case LogLevelDef.Error: return "ERR";
                case LogLevelDef.Fatal: return "FTL";
                default:
                    ThrowHelper.ThrowUnsupportedEnum(level);
                    return null;
            }
        }
    }
}