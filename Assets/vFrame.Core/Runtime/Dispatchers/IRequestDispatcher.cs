// ------------------------------------------------------------
//         File: IRequestDispatcher.cs
//        Brief: Request dispatcher interface
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    public interface IRequestDispatcher
    {
        /// <summary>
        ///     Sends a request and returns the response.
        /// </summary>
        /// <param name="payload">The request payload.</param>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <returns>The response from the handler; default value when no handler is registered.</returns>
        TResponse Request<TRequest, TResponse>(in TRequest payload)
            where TRequest : IRequest<TResponse>;

        /// <summary>
        ///     Tries to send a request and retrieve the response.
        /// </summary>
        /// <param name="payload">The request payload.</param>
        /// <param name="response">The output response value.</param>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <returns><c>true</c> if a handler exists and was invoked successfully; otherwise <c>false</c>.</returns>
        bool TryRequest<TRequest, TResponse>(in TRequest payload, out TResponse response)
            where TRequest : IRequest<TResponse>;

        /// <summary>
        ///     Registers a handler for the specified request type.
        ///     If a handler already exists, it is replaced (default <see cref="RegisterMode.Replace" />).
        /// </summary>
        /// <param name="handler">The request handler callback.</param>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <returns>A subscription handle.</returns>
        ISubscription HandleRequest<TRequest, TResponse>(Func<TRequest, TResponse> handler)
            where TRequest : IRequest<TResponse>;

        /// <summary>
        ///     Registers a handler for the specified request type with explicit register mode.
        /// </summary>
        /// <param name="handler">The request handler callback.</param>
        /// <param name="mode">The behavior when a handler already exists.</param>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <returns>A subscription handle, or <c>null</c> if <see cref="RegisterMode.Ignore" /> and a handler already exists.</returns>
        ISubscription HandleRequest<TRequest, TResponse>(Func<TRequest, TResponse> handler, RegisterMode mode)
            where TRequest : IRequest<TResponse>;

        /// <summary>
        ///     Registers a handler for the specified request type, bound to the owner's lifetime.
        /// </summary>
        /// <param name="handler">The request handler callback.</param>
        /// <param name="owner">The subscription owner; the subscription is automatically cancelled when the owner is destroyed.</param>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <returns>A subscription handle.</returns>
        ISubscription HandleRequest<TRequest, TResponse>(Func<TRequest, TResponse> handler, BaseObject owner)
            where TRequest : IRequest<TResponse>;

        /// <summary>
        ///     Registers a handler for the specified request type, bound to the given lifetime.
        /// </summary>
        /// <param name="handler">The request handler callback.</param>
        /// <param name="lifetime">The lifetime boundary; the subscription is automatically cancelled when the lifetime ends.</param>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <returns>A subscription handle.</returns>
        ISubscription HandleRequest<TRequest, TResponse>(Func<TRequest, TResponse> handler, ILifetime lifetime)
            where TRequest : IRequest<TResponse>;

        /// <summary>
        ///     Cancels the specified request handler subscription.
        /// </summary>
        /// <param name="subscription">The subscription handle to cancel.</param>
        void UnhandleRequest(ISubscription subscription);

        /// <summary>
        ///     Gets the current number of request subscriptions.
        /// </summary>
        /// <returns>The number of request subscriptions.</returns>
        int GetRequestSubscriptionCount();
    }
}