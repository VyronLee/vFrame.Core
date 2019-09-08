//------------------------------------------------------------
//       @file  IBindable.cs
//      @brief  可绑定类型接口
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-09-21 19:19
//   Copyright  Copyright (c) 2016, VyronLee
//============================================================

using System.Collections.Generic;
using vFrame.Core.Components;

namespace vFrame.Core.Interface.Components
{
    public interface IBindable
    {
        /// <summary>
        ///     绑定组件
        /// </summary>
        T AddComponent<T>() where T : Component, new();

        /// <summary>
        ///     绑定组件
        /// </summary>
        IComponent AddComponent(IComponent comp);

        /// <summary>
        ///     解绑组件
        /// </summary>
        void RemoveComponent<T>() where T : Component, new();

        /// <summary>
        ///     解绑所有组件
        /// </summary>
        void RemoveAllComponents();

        /// <summary>
        ///     获取组件
        /// </summary>
        T GetComponent<T>() where T : Component, new();

        /// <summary>
        ///     获取组件
        /// </summary>
        IComponent GetComponent(string name);

        /// <summary>
        ///     获取所有组件
        /// </summary>
        Dictionary<string, IComponent> GetAllComponents();

        /// <summary>
        ///     像所有组件广播消息
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="args">参数列表</param>
        void Broadcast(string method, params object[] args);
    }
}