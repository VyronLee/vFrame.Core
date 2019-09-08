//------------------------------------------------------------
//       @file  IComponent.cs
//      @brief  组件接口
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-09-21 19:18
//   Copyright  Copyright (c) 2016, VyronLee
//============================================================

namespace vFrame.Core.Interface.Components
{
    public interface IComponent
    {
        /// <summary>
        ///     绑定处理
        /// </summary>
        void BindTo(IBindable target);

        /// <summary>
        ///     解绑处理
        /// </summary>
        void UnBindFrom(IBindable target);

        /// <summary>
        ///     获取绑定目标
        /// </summary>
        IBindable GetTarget();
    }
}