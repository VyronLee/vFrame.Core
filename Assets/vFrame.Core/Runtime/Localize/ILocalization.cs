// ------------------------------------------------------------
//         File: ILocalization.cs
//        Brief: Interface for the localization manager.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-10-05 11:09:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    public interface ILocalization
    {
        /// <summary>
        ///     Gets or sets the current language code.
        /// </summary>
        string Language { get; set; }

        /// <summary>
        ///     Gets the localized text for the specified text ID.
        /// </summary>
        /// <param name="textId">The text identifier to look up.</param>
        /// <returns>The localized text content.</returns>
        string GetText(string textId);

        /// <summary>
        ///     Raised when the language setting changes.
        /// </summary>
        event Action<string> OnLocalize;
    }
}
