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

using vFrame.Core.Interface.Events;
using vFrame.Core.Pool;

namespace vFrame.Core.Events
{
    public class DelegateEventListener : Poolable, IEventListener
    {
        /// <summary>
        /// 代理接口
        /// </summary>
        public EventDelegate eventDelegate;

        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            eventDelegate = null;
        }

        /// <summary>
        /// 事件响应接口
        /// </summary>
        public void OnEvent(IEvent e)
        {
            if (null != eventDelegate)
            {
                eventDelegate(e);
            }
        }

        protected override void OnCreate()
        {

        }

        protected override void OnDestroy()
        {

        }
    }
}