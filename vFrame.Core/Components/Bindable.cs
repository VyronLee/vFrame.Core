//------------------------------------------------------------
//       @file  Bindable.cs
//      @brief  可绑定类型
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-07-28 11:00
//   Copyright  Copyright (c) 2016, VyronLee
//============================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using vFrame.Core.Base;
using vFrame.Core.Interface.Components;

namespace vFrame.Core.Components
{
    public abstract class Bindable : Component, IBindable
    {
        /// <summary>
        ///     已绑定组件映射
        /// </summary>
        private Dictionary<string, IComponent> _components;

        /// <summary>
        ///     绑定组件
        /// </summary>
        public T AddComponent<T>() where T : Component, new()
        {
            var typename = ComponentRegistry.GetComponentName<T>();
            Debug.Assert(!_components.ContainsKey(typename),
                "Bind component target already exist.");

            var comp = new T();
            comp.Create();

            _components[typename] = comp;
            comp.BindTo(this);
            return comp;
        }

        /// <summary>
        ///     绑定组件
        /// </summary>
        public IComponent AddComponent(IComponent comp)
        {
            var typename = comp.GetType().Name;
            _components[typename] = comp;
            comp.BindTo(this);
            return comp;
        }

        /// <summary>
        ///     解绑组件
        /// </summary>
        public void RemoveComponent<T>() where T : Component, new()
        {
            var typename = ComponentRegistry.GetComponentName<T>();
            Debug.Assert(_components.ContainsKey(typename),
                "Unbind component target doesn't exist.");

            var comp = _components[typename] as Component;
            comp.UnBindFrom(this);
            comp.Destroy();

            _components.Remove(typename);
        }

        /// <summary>
        ///     解绑所有组件
        /// </summary>
        public void RemoveAllComponents()
        {
            foreach (var item in _components)
                (item.Value as BaseObject).Destroy();
            _components = new Dictionary<string, IComponent>();
        }

        /// <summary>
        ///     获取组件
        /// </summary>
        public T GetComponent<T>() where T : Component, new()
        {
            var typename = ComponentRegistry.GetComponentName<T>();
            Debug.Assert(_components.ContainsKey(typename),
                "Component target doesn't exist. ");
            return _components[typename] as T;
        }

        /// <summary>
        ///     获取组件
        /// </summary>
        public IComponent GetComponent(string name)
        {
            Debug.Assert(_components.ContainsKey(name),
                "Component target doesn't exist.");
            return _components[name];
        }

        /// <summary>
        ///     获取所有组件
        /// </summary>
        public Dictionary<string, IComponent> GetAllComponents()
        {
            return _components;
        }

        /// <summary>
        ///     像所有组件广播消息
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="args">参数列表</param>
        public void Broadcast(string method, params object[] args)
        {
            var iter = _components.GetEnumerator();
            while (iter.MoveNext())
            {
                var comp = iter.Current.Value;
                var methodInfo = comp.GetType().GetMethod(method);
                if (null != methodInfo) methodInfo.Invoke(comp, args);
            }
        }

        /// <summary>
        ///     构造处理
        /// </summary>
        protected override void OnCreate()
        {
            _components = new Dictionary<string, IComponent>();
        }

        /// <summary>
        ///     析构处理
        /// </summary>
        protected override void OnDestroy()
        {
            RemoveAllComponents();
            _components = null;
        }


        /// <description>
        ///     组件注册器
        ///     - 主要用于维护一份组件类型与名称的映射，防止每次获取组件时做类型名生成
        /// </description>
        private static class ComponentRegistry
        {
            /// <summary>
            ///     类型映射
            /// </summary>
            private static readonly Dictionary<Type, string> TypeMap = new Dictionary<Type, string>();

            /// <summary>
            ///     获取组件名称
            /// </summary>
            public static string GetComponentName<TC>()
            {
                var type = typeof(TC);
                if (!TypeMap.ContainsKey(type))
                    TypeMap.Add(type, type.Name);
                return TypeMap[type];
            }
        }

    }
}