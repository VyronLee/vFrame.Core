// ------------------------------------------------------------
//         File: IContainer.cs
//        Brief: Interface for a container that manages components
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2016-09-21 19:19:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    public interface IContainer
    {
        /// <summary>
        /// Adds a component of the specified type to this container.
        /// </summary>
        /// <typeparam name="T">The component type to add.</typeparam>
        /// <returns>The added component instance.</returns>
        T AddComponent<T>() where T : Component;

        /// <summary>
        /// Adds a component of the specified type to this container.
        /// </summary>
        /// <param name="type">The component type to add.</param>
        /// <returns>The added component instance.</returns>
        IComponent AddComponent(Type type);

        /// <summary>
        /// Removes the component of the specified type from this container.
        /// </summary>
        /// <typeparam name="T">The component type to remove.</typeparam>
        void RemoveComponent<T>() where T : Component;

        /// <summary>
        /// Removes the component of the specified type from this container.
        /// </summary>
        /// <param name="type">The component type to remove.</param>
        void RemoveComponent(Type type);

        /// <summary>
        /// Removes all components from this container.
        /// </summary>
        void RemoveAllComponents();

        /// <summary>
        /// Gets the component of the specified type from this container.
        /// </summary>
        /// <typeparam name="T">The component type to retrieve.</typeparam>
        /// <returns>The component instance, or null if not found.</returns>
        T GetComponent<T>() where T : Component;

        /// <summary>
        /// Gets the component of the specified type from this container.
        /// </summary>
        /// <param name="type">The component type to retrieve.</param>
        /// <returns>The component instance.</returns>
        IComponent GetComponent(Type type);

        /// <summary>
        /// Gets all components registered in this container.
        /// </summary>
        /// <returns>An array of all registered components.</returns>
        IComponent[] GetAllComponents();

        /// <summary>
        /// Broadcasts a message to all components by invoking a named method on each.
        /// </summary>
        /// <param name="method">The name of the method to invoke on each component.</param>
        /// <param name="args">The arguments to pass to the invoked method.</param>
        void Broadcast(string method, params object[] args);

        /// <summary>
        /// Sends a loopback call that returns the result from the first component
        /// that has a method matching the given name.
        /// </summary>
        /// <param name="method">The name of the method to invoke.</param>
        /// <param name="args">The arguments to pass to the invoked method.</param>
        /// <returns>The return value from the first matching component, or null if none found.</returns>
        object Loopback(string method, params object[] args);
    }
}
