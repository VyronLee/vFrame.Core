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

using vFrame.Core.Base;

namespace vFrame.Core.Events
{
    public class EventExecutor : BaseObject
    {
        public bool Activated { get; set; }
        public bool Stopped { get; set; }

        public int EventId;
        public uint Handle;
        public IEventListener Listener;

        /// <summary>
        ///     创建函数
        /// </summary>
        protected override void OnCreate() {
        }

        /// <summary>
        /// 销毁函数
        /// </summary>
        protected override void OnDestroy() {
            Listener = null;
        }

        /// <summary>
        ///     激活
        /// </summary>
        public void Activate() {
            Activated = true;
        }

        /// <summary>
        ///     停止
        /// </summary>
        public void Stop() {
            Stopped = true;
        }

        /// <summary>
        ///     执行
        /// </summary>
        public void Execute(IEvent e) {
            if (null != Listener)
                Listener.OnEvent(e);
        }
    }
}