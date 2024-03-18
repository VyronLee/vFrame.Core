//------------------------------------------------------------
//        File:  Localization.cs
//       Brief:  多语言管理器
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-30 20:32
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System;
using System.Collections.Generic;
using vFrame.Core.Base;
using vFrame.Core.Loggers;
using vFrame.Core.ThirdParty.LitJson;

namespace vFrame.Core.Localize
{
    public class Localization : BaseObject<ILocalizationReader>, ILocalization
    {
        private static readonly LogTag LogTag = new LogTag("Localization");

        /// <summary>
        /// 当前语言代码
        /// </summary>
        private string _langCode = "zh_CN";

        /// <summary>
        /// 各语言TextID映射
        /// </summary>
        private Dictionary<string, JsonData> _langTextIdMap;

        /// <summary>
        /// 多语言设置变更通知
        /// </summary>
        public event Action<string> OnLocalize;

        /// <summary>
        /// 数据读取器
        /// </summary>
        private ILocalizationReader _reader;

        /// <summary>
        /// 创建
        /// </summary>
        protected override void OnCreate(ILocalizationReader arg1) {
            _langTextIdMap = new Dictionary<string, JsonData>();
            _reader = arg1;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        protected override void OnDestroy() {
            _reader = null;
            _langTextIdMap = null;
        }

        /// <summary>
        /// 获取/设置语言代码
        /// </summary>
        public string Language {
            get => _langCode;
            set {
                var changed = _langCode != value;
                _langCode = value;

                if (changed && null != OnLocalize)
                    OnLocalize.Invoke(_langCode);
            }
        }

        /// <summary>
        /// 获取文本
        /// </summary>
        /// <param name="textId"></param>
        /// <returns>文本内容</returns>
        public string GetText(string textId) {
            LazyLoad();

            JsonData lang;
            if (!_langTextIdMap.TryGetValue(Language, out lang))
                return string.Empty;

            if (!lang.ContainsKey(textId)) {
                Logger.Error(LogTag, "No text Id defined in dict: {0}", textId);
                return string.Empty;
            }

            return lang[textId].ToString();
        }

        /// <summary>
        /// 懒加载
        /// </summary>
        private void LazyLoad() {
            if (_langTextIdMap.ContainsKey(Language))
                return;

            LoadLanguage(Language);
        }

        /// <summary>
        /// 加载语言数据
        /// </summary>
        /// <param name="lang"></param>
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