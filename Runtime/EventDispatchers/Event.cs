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

using vFrame.Core.Base;

namespace vFrame.Core.EventDispatchers
{
    public class Event : BaseObject, IEvent
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

        /// <summary>
        ///     创建函数
        /// </summary>
        protected override void OnCreate() { }

        /// <summary>
        ///     销毁函数
        /// </summary>
        protected override void OnDestroy() {
            Target = null;
        }
    }
}