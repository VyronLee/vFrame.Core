//------------------------------------------------------------
//       @file  Event.cs
//      @brief  事件
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//     Created  2016-08-03 18:03
//   Copyright  Copyright (c) 2024, VyronLee
//============================================================

namespace vFrame.Core.EventDispatchers
{
    public class Event : IEvent
    {
        public object Context;
        public int EventId;
        public EventDispatcher Target;

        /// <summary>
        ///     获取事件ID
        /// </summary>
        public int GetEventID() {
            return EventId;
        }

        /// <summary>
        ///     获取事件现场
        /// </summary>
        public object GetContext() {
            return Context;
        }

        /// <summary>
        ///     获取事件发送者
        /// </summary>
        public object GetEventTarget() {
            return Target;
        }

    }
}
