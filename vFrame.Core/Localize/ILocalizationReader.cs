//------------------------------------------------------------
//        File:  ILocalizationReader.cs
//       Brief:  多语言数据读取接口
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-09-30 21:05
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================
namespace vFrame.Core.Localize
{
    public interface ILocalizationReader
    {
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="langCode">语言代码</param>
        /// <returns>数据文本</returns>
        string ReadData(string langCode);
    }
}