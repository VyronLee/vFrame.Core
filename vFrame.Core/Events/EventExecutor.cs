//------------------------------------------------------------
//       @file  EventExecutor.cs
//      @brief  事件执行者
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-08-01 15:56
//   Copyright  Copyright (c) 2016, VyronLee
//============================================================

using vFrame.Core.Interface.Events;
using vFrame.Core.Pool;

namespace vFrame.Core.Events
{
    public class EventExecutor : Poolable
    {
        public bool activated;
        public int eventId;
        public int handle;
        public IEventListener listener;
        public int priority;
        public bool stopped;

        /// <summary>
        ///     创建函数
        /// </summary>
        protected override void OnCreate()
        {
            Reset();
        }

        /// <summary>
        /// 销毁函数
        /// </summary>
        protected override void OnDestroy()
        {
            Reset();
        }

        /// <summary>
        ///     重置
        /// </summary>
        public override void Reset()
        {
            handle = 0;
            eventId = 0;
            priority = 0;
            listener = null;
            activated = false;
            stopped = false;
        }

        /// <summary>
        ///     激活
        /// </summary>
        public void Activate()
        {
            activated = true;
        }

        /// <summary>
        ///     是否已经激活
        /// </summary>
        public bool IsActivated()
        {
            return activated;
        }

        /// <summary>
        ///     停止
        /// </summary>
        public void Stop()
        {
            stopped = true;
        }

        /// <summary>
        ///     是否已经停止
        /// </summary>
        public bool IsStopped()
        {
            return stopped;
        }

        /// <summary>
        ///     执行
        /// </summary>
        public void Execute(IEvent e)
        {
            if (null != listener)
                listener.OnEvent(e);
        }

    }
}