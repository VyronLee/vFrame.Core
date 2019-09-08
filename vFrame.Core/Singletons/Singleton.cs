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
    public abstract class Singleton<T> : BaseObject where T: BaseObject, new()
    {
        /// <summary>
        ///     单例对象
        /// </summary>
        private static T _instance;

        protected Singleton()
        {
            Debug.Assert(null == _instance, "Singleton duplicate.");
            _instance = this as T;
        }

        /// <summary>
        ///     获取单例
        /// </summary>
        public static T Instance()
        {
            _instance = _instance ?? NewInstance();
            return _instance;
        }

        /// <summary>
        ///     新建单例
        /// </summary>
        public static T NewInstance()
        {
            Debug.Assert(null == _instance, "Singleton duplicate.");

            _instance = new T();
            _instance.Create();
            return _instance;
        }
    }
}