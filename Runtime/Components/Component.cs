//------------------------------------------------------------
//       @file  Component.cs
//      @brief  组件类型
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//     Created  2016-07-28 10:54
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
        ///     获取绑定目标
        /// </summary>
        public IContainer GetContainer() {
            return _container;
        }

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
        ///     发送事件
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        public void SendEvent(string methodName, params object[] args) {
            _container?.Broadcast(methodName, args);
        }

        /// <summary>
        ///     发送命令
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object SendCommand(string methodName, params object[] args) {
            return _container?.Loopback(methodName, args);
        }

        /// <summary>
        ///     绑定处理
        /// </summary>
        protected virtual void OnBind(IContainer target) { }

        /// <summary>
        ///     解绑处理
        /// </summary>
        protected virtual void OnUnbind(IContainer target) { }
    }
}