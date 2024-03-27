//------------------------------------------------------------
//       @file  IEventListener.cs
//      @brief  事件侦听器
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//     Created  2016-07-31 22:25
//   Copyright  Copyright (c) 2024, VyronLee
//============================================================

namespace vFrame.Core.EventDispatchers
{
    public interface IEventListener
    {
        /// <summary>
        ///     事件响应接口
        /// </summary>
        void OnEvent(IEvent e);
    }
}