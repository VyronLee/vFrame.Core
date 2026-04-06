//------------------------------------------------------------
//       @file  EventExecutor.cs
//      @brief  事件执行者
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//     Created  2016-08-01 15:56
//   Copyright  Copyright (c) 2024, VyronLee
//============================================================

namespace vFrame.Core.EventDispatchers
{
    public class EventExecutor
    {
        public int EventId;
        public uint Handle;
        public IEventListener Listener;
        public bool Activated { get; set; }
        public bool Stopped { get; set; }

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
            if (null != Listener) {
                Listener.OnEvent(e);
            }
        }
    }
}
