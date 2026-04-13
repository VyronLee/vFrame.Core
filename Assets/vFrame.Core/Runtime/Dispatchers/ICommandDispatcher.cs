// ------------------------------------------------------------
//         File: ICommandDispatcher.cs
//        Brief: Command dispatcher interface
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    public interface ICommandDispatcher
    {
        /// <summary>
        ///     Registers a handler for the specified command type.
        ///     If a handler already exists, it is replaced (default <see cref="RegisterMode.Replace" />).
        /// </summary>
        /// <param name="handler">The command handler callback.</param>
        /// <typeparam name="TCommand">The command type.</typeparam>
        /// <returns>A subscription handle.</returns>
        ISubscription Handle<TCommand>(Action<TCommand> handler)
            where TCommand : ICommand;

        /// <summary>
        ///     Registers a handler for the specified command type with explicit register mode.
        /// </summary>
        /// <param name="handler">The command handler callback.</param>
        /// <param name="mode">The behavior when a handler already exists.</param>
        /// <typeparam name="TCommand">The command type.</typeparam>
        /// <returns>A subscription handle, or <c>null</c> if <see cref="RegisterMode.Ignore" /> and a handler already exists.</returns>
        ISubscription Handle<TCommand>(Action<TCommand> handler, RegisterMode mode)
            where TCommand : ICommand;

        /// <summary>
        ///     Registers a handler for the specified command type, bound to the owner's lifetime.
        /// </summary>
        /// <param name="handler">The command handler callback.</param>
        /// <param name="owner">The subscription owner; the subscription is automatically cancelled when the owner is destroyed.</param>
        /// <typeparam name="TCommand">The command type.</typeparam>
        /// <returns>A subscription handle.</returns>
        ISubscription Handle<TCommand>(Action<TCommand> handler, BaseObject owner)
            where TCommand : ICommand;

        /// <summary>
        ///     Registers a handler for the specified command type, bound to the given lifetime.
        /// </summary>
        /// <param name="handler">The command handler callback.</param>
        /// <param name="lifetime">The lifetime boundary; the subscription is automatically cancelled when the lifetime ends.</param>
        /// <typeparam name="TCommand">The command type.</typeparam>
        /// <returns>A subscription handle.</returns>
        ISubscription Handle<TCommand>(Action<TCommand> handler, ILifetime lifetime)
            where TCommand : ICommand;

        /// <summary>
        ///     Cancels the specified command handler subscription.
        /// </summary>
        /// <param name="subscription">The subscription handle to cancel.</param>
        void Unhandle(ISubscription subscription);

        /// <summary>
        ///     Sends a command of the specified type to be executed by the registered handler.
        /// </summary>
        /// <param name="command">The command payload.</param>
        /// <typeparam name="TCommand">The command type.</typeparam>
        void Send<TCommand>(in TCommand command)
            where TCommand : ICommand;

        /// <summary>
        ///     Gets the current number of command subscriptions.
        /// </summary>
        /// <returns>The number of command subscriptions.</returns>
        int GetCommandSubscriptionCount();
    }
}