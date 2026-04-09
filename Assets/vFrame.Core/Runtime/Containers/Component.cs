// ------------------------------------------------------------
//         File: Component.cs
//        Brief: Abstract base class for bindable components
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2016-07-28 10:54:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.Diagnostics;
using vFrame.Core;

namespace vFrame.Core
{
    public abstract class Component : BaseObject, IComponent
    {
        private IContainer _container;

        /// <summary>
        /// Gets the container to which this component is bound.
        /// </summary>
        /// <returns>The owning container instance.</returns>
        public IContainer GetContainer() {
            return _container;
        }

        /// <summary>
        /// Binds this component to the specified container target.
        /// </summary>
        /// <param name="target">The container to bind to.</param>
        public void BindTo(IContainer target) {
            _container = target;
            OnBind(target);
        }

        /// <summary>
        /// Unbinds this component from the specified container target.
        /// Asserts that the target matches the current container.
        /// </summary>
        /// <param name="target">The container to unbind from.</param>
        public void UnBindFrom(IContainer target) {
            Debug.Assert(_container == target, "Unbind target is not the same as parent.");

            OnUnbind(target);
            _container = null;
        }

        /// <summary>
        /// Broadcasts an event to all components in the owning container.
        /// </summary>
        /// <param name="methodName">The name of the method to invoke on each component.</param>
        /// <param name="args">The arguments to pass to the invoked method.</param>
        public void SendEvent(string methodName, params object[] args) {
            _container?.Broadcast(methodName, args);
        }

        /// <summary>
        /// Sends a command to the owning container and returns the result
        /// from the first component that handles it.
        /// </summary>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="args">The arguments to pass to the invoked method.</param>
        /// <returns>The return value from the handler, or null if no container or handler is found.</returns>
        public object SendCommand(string methodName, params object[] args) {
            return _container?.Loopback(methodName, args);
        }

        /// <summary>
        /// Called when this component is bound to a container.
        /// Override to perform custom bind logic.
        /// </summary>
        /// <param name="target">The container this component was bound to.</param>
        protected virtual void OnBind(IContainer target) { }

        /// <summary>
        /// Called when this component is unbound from a container.
        /// Override to perform custom unbind logic.
        /// </summary>
        /// <param name="target">The container this component was unbound from.</param>
        protected virtual void OnUnbind(IContainer target) { }
    }
}
