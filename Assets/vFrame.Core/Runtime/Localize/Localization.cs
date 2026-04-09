// ------------------------------------------------------------
//         File: Localization.cs
//        Brief: Localization manager that provides multilingual text lookup.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-30 20:32:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Generic;

namespace vFrame.Core
{
    public class Localization : BaseObject<ILocalizationReader>, ILocalization
    {
        private static readonly LogTag LogTag = new LogTag("Localization");

        /// <summary>
        ///     Current language code.
        /// </summary>
        private string _langCode = "zh_CN";

        /// <summary>
        ///     Language code to text ID mapping.
        /// </summary>
        private Dictionary<string, JsonData> _langTextIdMap;

        /// <summary>
        ///     Data reader for localization content.
        /// </summary>
        private ILocalizationReader _reader;

        /// <summary>
        ///     Notifies when the language setting changes.
        /// </summary>
        public event Action<string> OnLocalize;

        /// <summary>
        ///     Gets or sets the current language code.
        /// </summary>
        public string Language {
            get => _langCode;
            set {
                var changed = _langCode != value;
                _langCode = value;

                if (changed && null != OnLocalize) {
                    OnLocalize.Invoke(_langCode);
                }
            }
        }

        /// <summary>
        ///     Gets the localized text for the specified text ID.
        /// </summary>
        /// <param name="textId">The text identifier to look up.</param>
        /// <returns>The localized text content, or empty string if not found.</returns>
        public string GetText(string textId) {
            LazyLoad();

            JsonData lang;
            if (!_langTextIdMap.TryGetValue(Language, out lang)) {
                return string.Empty;
            }

            if (!lang.ContainsKey(textId)) {
                Logger.Error(LogTag, "No text Id defined in dict: {0}", textId);
                return string.Empty;
            }

            return lang[textId].ToString();
        }

        /// <summary>
        ///     Called when the instance is created with the given reader.
        /// </summary>
        /// <param name="arg1">The localization reader providing raw data.</param>
        protected override void OnCreate(ILocalizationReader arg1) {
            _langTextIdMap = new Dictionary<string, JsonData>();
            _reader = arg1;
        }

        /// <summary>
        ///     Called when the instance is destroyed. Releases held references.
        /// </summary>
        protected override void OnDestroy() {
            _reader = null;
            _langTextIdMap = null;
        }

        /// <summary>
        ///     Lazily loads language data on first access.
        /// </summary>
        private void LazyLoad() {
            if (_langTextIdMap.ContainsKey(Language)) {
                return;
            }

            LoadLanguage(Language);
        }

        /// <summary>
        ///     Loads and parses localization data for the specified language.
        /// </summary>
        /// <param name="lang">The language code to load.</param>
        private void LoadLanguage(string lang) {
            var data = _reader.ReadData(lang);
            if (string.IsNullOrEmpty(data)) {
                Logger.Error(LogTag, "Read localization data failed: " + lang);
                return;
            }

            try {
                _langTextIdMap[lang] = JsonMapper.ToObject(data);
            }
            catch (Exception e) {
                Logger.Error(LogTag, "Parse localization data failed: {0}, exception: {1}", lang, e);
            }
        }
    }
}