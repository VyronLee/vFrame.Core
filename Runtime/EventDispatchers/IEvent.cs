//------------------------------------------------------------
//       @file  IEvent.cs
//      @brief  事件接口
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//     Created  2016-07-31 22:18
//   Copyright  Copyright (c) 2024, VyronLee
//============================================================

namespace vFrame.Core.EventDispatchers
{
    public interface IEvent
    {
        /// <summary>
        ///     获取事件ID
        /// </summary>
        int GetEventID();

        /// <summary>
        ///     获取事件现场
        /// </summary>
        object GetContext();

        /// <summary>
        ///     获取事件发送者
        /// </summary>
        object GetEventTarget();
    }
}