//------------------------------------------------------------
//       @file  Bindable.cs
//      @brief  可绑定类型
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//     Created  2016-07-28 11:00
//   Copyright  Copyright (c) 2024, VyronLee
//============================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using vFrame.Core.Loggers;

namespace vFrame.Core.Components
{
    public abstract class Container : Component, IContainer
    {
        /// <summary>
        ///     已绑定组件映射
        /// </summary>
        private Dictionary<Type, Component> _components;

        /// <summary>
        ///     绑定组件
        /// </summary>
        public T AddComponent<T>() where T : Component {
            var type = typeof(T);
            return AddComponent(type) as T;
        }

        /// <summary>
        ///     绑定组件
        /// </summary>
        public IComponent AddComponent(Type type) {
            Debug.Assert(type.IsSubclassOf(typeof(Component)));

            if (_components.TryGetValue(type, out var component)) {
                Logger.Error("Component '{0}' has already exist.", type.Name);
                return component;
            }

            var comp = Activator.CreateInstance(type) as Component;
            if (null == comp) {
                return null;
            }

            _components[type] = comp;
            comp.BindTo(this);
            return comp;
        }

        /// <summary>
        ///     解绑组件
        /// </summary>
        public void RemoveComponent<T>() where T : Component {
            var type = typeof(T);
            RemoveComponent(type);
        }

        /// <summary>
        ///     解绑组件
        /// </summary>
        public void RemoveComponent(Type type) {
            Component component;
            if (!_components.TryGetValue(type, out component)) {
                return;
            }

            component.UnBindFrom(this);
            component.Destroy();

            _components.Remove(type);
        }

        /// <summary>
        ///     解绑所有组件
        /// </summary>
        public void RemoveAllComponents() {
            foreach (var item in _components) {
                item.Value.Destroy();
            }
            _components.Clear();
        }

        /// <summary>
        ///     获取组件
        /// </summary>
        public T GetComponent<T>() where T : Component {
            var type = typeof(T);
            if (_components.TryGetValue(type, out var component)) {
                return component as T;
            }
            return null;
        }

        /// <summary>
        ///     获取组件
        /// </summary>
        public IComponent GetComponent(Type type) {
            return _components[type];
        }

        /// <summary>
        ///     获取所有组件
        /// </summary>
        public IComponent[] GetAllComponents() {
            var components = new IComponent[_components.Count];
            var index = 0;
            foreach (var kv in _components) {
                components[index++] = kv.Value;
            }
            return components;
        }

        /// <summary>
        ///     广播调用
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="args">参数列表</param>
        public void Broadcast(string method, params object[] args) {
            foreach (var kv in _components) {
                var comp = kv.Value;
                var methodInfo = comp.GetType().GetMethod(method,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (null != methodInfo) {
                    methodInfo.Invoke(comp, args);
                }
            }
        }

        /// <summary>
        ///     回送调用
        /// </summary>
        /// <param name="method">函数名</param>
        /// <param name="args">参数列表</param>
        public object Loopback(string method, params object[] args) {
            foreach (var kv in _components) {
                var comp = kv.Value;
                var methodInfo = comp.GetType().GetMethod(method,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (null != methodInfo) {
                    return methodInfo.Invoke(comp, args);
                }
            }
            return null;
        }

        /// <summary>
        ///     构造处理
        /// </summary>
        protected override void OnCreate() {
            _components = new Dictionary<Type, Component>();
        }

        /// <summary>
        ///     析构处理
        /// </summary>
        protected override void OnDestroy() {
            RemoveAllComponents();
            _components = null;
        }
    }
}