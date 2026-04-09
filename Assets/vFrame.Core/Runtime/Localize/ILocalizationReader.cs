// ------------------------------------------------------------
//         File: ILocalizationReader.cs
//        Brief: Interface for reading localization data.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-30 21:05:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    public interface ILocalizationReader
    {
        /// <summary>
        ///     Reads the raw localization data for the specified language code.
        /// </summary>
        /// <param name="langCode">The language code to read data for.</param>
        /// <returns>The raw data text, or null/empty if unavailable.</returns>
        string ReadData(string langCode);
    }
}
