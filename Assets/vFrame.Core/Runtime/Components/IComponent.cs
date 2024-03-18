//------------------------------------------------------------
//       @file  IComponent.cs
//      @brief  组件接口
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-09-21 19:18
//   Copyright  Copyright (c) 2024, VyronLee
//============================================================

namespace vFrame.Core.Components
{
    public interface IComponent
    {
        /// <summary>
        ///     获取绑定目标
        /// </summary>
        IContainer GetContainer();
    }
}