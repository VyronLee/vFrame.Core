//------------------------------------------------------------
//       @file  Component.cs
//      @brief  组件类型
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-07-28 10:54
//   Copyright  Copyright (c) 2024, VyronLee
//============================================================

using System.Diagnostics;
using vFrame.Core.Base;

namespace vFrame.Core.Components
{
    public abstract class Component : BaseObject, IComponent
    {
        /// <summary>
        ///     绑定对象
        /// </summary>
        private IContainer _container;

        /// <summary>
        ///     绑定处理
        /// </summary>
        public void BindTo(IContainer target) {
            _container = target;
            OnBind(target);
        }

        /// <summary>
        ///     解绑处理
        /// </summary>
        public void UnBindFrom(IContainer target) {
            Debug.Assert(_container == target, "Unbind target is not the same as parent.");

            OnUnbind(target);
            _container = null;
        }

        /// <summary>
        ///     获取绑定目标
        /// </summary>
        public IContainer GetContainer() {
            return _container;
        }

        /// <summary>
        ///     绑定处理
        /// </summary>
        protected virtual void OnBind(IContainer target) {
        }

        /// <summary>
        ///     解绑处理
        /// </summary>
        protected virtual void OnUnbind(IContainer target) {
        }
    }
}