//------------------------------------------------------------
//       @file  Component.cs
//      @brief  组件类型
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-07-28 10:54
//   Copyright  Copyright (c) 2016, VyronLee
//============================================================

using System.Diagnostics;
using vFrame.Core.Base;
using vFrame.Core.Interface.Components;

namespace vFrame.Core.Components
{
    public abstract class Component : BaseObject, IComponent
    {
        /// <summary>
        ///     绑定对象
        /// </summary>
        private IBindable _parent;

        /// <summary>
        ///     绑定处理
        /// </summary>
        public void BindTo(IBindable target)
        {
            _parent = target;
            OnBind(target);
        }

        /// <summary>
        ///     解绑处理
        /// </summary>
        public void UnBindFrom(IBindable target)
        {
            Debug.Assert(_parent == target, "Unbind target is not the same as parent.");

            OnUnbind(target);
            _parent = null;
        }

        /// <summary>
        ///     获取绑定目标
        /// </summary>
        public IBindable GetTarget()
        {
            return _parent;
        }

        /// <summary>
        ///     绑定处理
        /// </summary>
        protected abstract void OnBind(IBindable target);

        /// <summary>
        ///     解绑处理
        /// </summary>
        protected abstract void OnUnbind(IBindable target);
    }
}