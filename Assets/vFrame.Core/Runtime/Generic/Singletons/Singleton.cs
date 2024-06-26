﻿//------------------------------------------------------------
//       @file  Singleton.cs
//      @brief  单件模板
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//     Created  2016-07-28 15:19
//   Copyright  Copyright (c) 2024, VyronLee
//============================================================

using vFrame.Core.Base;

namespace vFrame.Core.Generic
{
    public abstract class Singleton<T> : BaseObject where T : BaseObject, new()
    {
        private static T _instance;

        private static readonly object _lockObject = new object();

        /// <summary>
        ///     销毁函数
        /// </summary>
        protected override void OnDestroy() {
            if (_instance == this) {
                _instance = null;
            }
        }

        /// <summary>
        ///     获取单例
        /// </summary>
        public static T Instance() {
            if (null == _instance) {
                lock (_lockObject) {
                    if (null == _instance) {
                        _instance = NewInstance();
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        ///     新建单例
        /// </summary>
        private static T NewInstance() {
            var instance = new T();
            instance.Create();
            return instance;
        }
    }
}