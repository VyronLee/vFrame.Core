//------------------------------------------------------------
//       @file  Singleton.cs
//      @brief  单件模板
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-07-28 15:19
//   Copyright  Copyright (c) 2016, VyronLee
//============================================================

using System.Diagnostics;
using vFrame.Core.Base;

namespace vFrame.Core.Singletons
{
    public abstract class Singleton<T> : BaseObject where T : BaseObject, new()
    {
        private static T _instance;

        /// <summary>
        /// 构造函数
        /// </summary>
        protected Singleton() {
            Debug.Assert(null == _instance, "Singleton duplicate.");
            _instance = this as T;
        }

        /// <summary>
        /// 销毁函数
        /// </summary>
        protected override void OnDestroy() {
            if (_instance == this)
                _instance = null;
        }

        /// <summary>
        ///     获取单例
        /// </summary>
        public static T Instance() {
            return _instance ?? (_instance = NewInstance());
        }

        /// <summary>
        ///     新建单例
        /// </summary>
        public static T NewInstance() {
            _instance = new T();
            _instance.Create();
            return _instance;
        }
    }
}