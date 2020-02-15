//------------------------------------------------------------
//        File:  ILocalization.cs
//       Brief:  多语言管理器接口
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-10-05 11:09
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System;

namespace vFrame.Core.Localize
{
    public interface ILocalization
    {
        /// <summary>
        /// 获取/设置语言代码
        /// </summary>
        string Language { get; set; }

        /// <summary>
        /// 获取文本
        /// </summary>
        /// <param name="textId"></param>
        /// <returns>文本内容</returns>
        string GetText(string textId);

        /// <summary>
        /// 语言设置变更回调
        /// </summary>
        event Action<string> OnLocalize;
    }
}