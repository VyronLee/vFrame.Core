// ------------------------------------------------------------
//         File: Container.cs
//        Brief: Abstract container that manages a collection of components
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2016-07-28 11:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using vFrame.Core;

namespace vFrame.Core
{
    public abstract class Container : Component, IContainer
    {
        private Dictionary<Type, Component> _components;

        /// <summary>
        /// Adds a component of the specified type to this container.
        /// </summary>
        /// <typeparam name="T">The component type to add.</typeparam>
        /// <returns>The added component instance.</returns>
        public T AddComponent<T>() where T : Component {
            var type = typeof(T);
            return AddComponent(type) as T;
        }

        /// <summary>
        /// Adds a component of the specified type to this container.
        /// Returns the existing component if one of the same type is already registered.
        /// </summary>
        /// <param name="type">The component type to add.</param>
        /// <returns>The added component instance, or null if instantiation failed.</returns>
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
        /// Removes the component of the specified type from this container.
        /// </summary>
        /// <typeparam name="T">The component type to remove.</typeparam>
        public void RemoveComponent<T>() where T : Component {
            var type = typeof(T);
            RemoveComponent(type);
        }

        /// <summary>
        /// Removes the component of the specified type from this container.
        /// Does nothing if no component of the given type is registered.
        /// </summary>
        /// <param name="type">The component type to remove.</param>
        public void RemoveComponent(Type type) {
            if (!_components.TryGetValue(type, out var component)) {
                return;
            }
            component.UnBindFrom(this);
            component.Destroy();

            _components.Remove(type);
        }

        /// <summary>
        /// Removes all components from this container and destroys them.
        /// </summary>
        public void RemoveAllComponents() {
            foreach (var item in _components) {
                item.Value.Destroy();
            }
            _components.Clear();
        }

        /// <summary>
        /// Gets the component of the specified type from this container.
        /// </summary>
        /// <typeparam name="T">The component type to retrieve.</typeparam>
        /// <returns>The component instance, or null if not found.</returns>
        public T GetComponent<T>() where T : Component {
            var type = typeof(T);
            if (_components.TryGetValue(type, out var component)) {
                return component as T;
            }
            return null;
        }

        /// <summary>
        /// Gets the component of the specified type from this container.
        /// </summary>
        /// <param name="type">The component type to retrieve.</param>
        /// <returns>The component instance.</returns>
        public IComponent GetComponent(Type type) {
            return _components[type];
        }

        /// <summary>
        /// Gets all components registered in this container.
        /// </summary>
        /// <returns>An array of all registered components.</returns>
        public IComponent[] GetAllComponents() {
            var components = new IComponent[_components.Count];
            var index = 0;
            foreach (var kv in _components) {
                components[index++] = kv.Value;
            }
            return components;
        }

        /// <summary>
        /// Broadcasts a message to all components by invoking a named method on each.
        /// </summary>
        /// <param name="method">The name of the method to invoke on each component.</param>
        /// <param name="args">The arguments to pass to the invoked method.</param>
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
        /// Sends a loopback call that returns the result from the first component
        /// that has a method matching the given name.
        /// </summary>
        /// <param name="method">The name of the method to invoke.</param>
        /// <param name="args">The arguments to pass to the invoked method.</param>
        /// <returns>The return value from the first matching component, or null if none found.</returns>
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
        /// Called when the container is created. Initializes the component dictionary.
        /// </summary>
        protected override void OnCreate() {
            _components = new Dictionary<Type, Component>();
        }

        /// <summary>
        /// Called when the container is destroyed. Removes all components and clears the dictionary.
        /// </summary>
        protected override void OnDestroy() {
            RemoveAllComponents();
            _components = null;
        }
    }
}
