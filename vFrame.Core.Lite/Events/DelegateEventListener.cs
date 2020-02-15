//------------------------------------------------------------
//       @file  DelegateEventListener.cs
//      @brief  代理事件侦听器
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-09-20 20:35
//   Copyright  Copyright (c) 2016, VyronLee
//============================================================

using System;
using vFrame.Core.Base;

namespace vFrame.Core.Events
{
    public class DelegateEventListener : BaseObject, IEventListener
    {
        /// <summary>
        /// 代理接口
        /// </summary>
        public Action<IEvent> Action;

        /// <summary>
        /// 事件响应接口
        /// </summary>
        public void OnEvent(IEvent e) {
            if (null != Action) {
                Action(e);
            }
        }

        protected override void OnCreate() {
        }

        protected override void OnDestroy() {
            Action = null;
        }
    }
}