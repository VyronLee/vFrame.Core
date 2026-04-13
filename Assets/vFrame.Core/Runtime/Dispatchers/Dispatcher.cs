// ------------------------------------------------------------
//         File: Dispatcher.cs
//        Brief: Dispatcher implementation aggregating event, command, request and decision dispatching
//                with priority ordering, register modes, and interceptor pipeline
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Generic;

namespace vFrame.Core
{
    public class Dispatcher : BaseObject, IDispatcher
    {
        /// <summary>
        /// Immutable diagnostics snapshot of subscription counts across all dispatching categories.
        /// </summary>
        public readonly struct DiagnosticsSnapshot
        {
            /// <summary>
            /// Creates a new diagnostics snapshot with the specified subscription counts.
            /// </summary>
            /// <param name="eventCount">The number of event subscriptions.</param>
            /// <param name="commandCount">The number of command subscriptions.</param>
            /// <param name="requestCount">The number of request subscriptions.</param>
            /// <param name="decisionCount">The number of decision subscriptions.</param>
            public DiagnosticsSnapshot(int eventCount, int commandCount, int requestCount, int decisionCount) {
                EventSubscriptionCount = eventCount;
                CommandSubscriptionCount = commandCount;
                RequestSubscriptionCount = requestCount;
                DecisionSubscriptionCount = decisionCount;
            }

            public int EventSubscriptionCount { get; }
            public int CommandSubscriptionCount { get; }
            public int RequestSubscriptionCount { get; }
            public int DecisionSubscriptionCount { get; }
        }

        private static readonly LogTag LogTag = new LogTag("Dispatcher");

        private uint _index;
        private uint _registrationOrder;
        private Dictionary<Type, List<Subscription>> _eventSubscriptions;
        private Dictionary<Type, Subscription> _commandSubscriptions;
        private Dictionary<Type, Subscription> _requestSubscriptions;
        private Dictionary<Type, List<Subscription>> _decisionSubscriptions;
        private SubscriptionPool _subscriptionPool;
        private List<IEventInterceptor> _interceptors;
        private HashSet<Type> _dirtyEventTypes;
        private HashSet<Type> _dirtyDecisionTypes;

        #region IEventDispatcher

        /// <summary>
        /// Subscribes to events of the specified type with default priority (0).
        /// </summary>
        /// <param name="action">The event handler callback.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <returns>A subscription handle.</returns>
        public ISubscription Subscribe<TEvent>(Action<TEvent> action)
            where TEvent : IEvent {
            return SubscribeInternal(action, 0, null);
        }

        /// <summary>
        /// Subscribes to events of the specified type, bound to the owner's lifetime.
        /// </summary>
        /// <param name="action">The event handler callback.</param>
        /// <param name="owner">The subscription owner; the subscription is automatically cancelled when the owner is destroyed.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <returns>A subscription handle.</returns>
        public ISubscription Subscribe<TEvent>(Action<TEvent> action, BaseObject owner)
            where TEvent : IEvent {
            ThrowHelper.ThrowIfNull(owner, nameof(owner));
            var subscription = SubscribeInternal(action, 0, null);
            owner.OwnLifetime(subscription);
            return subscription;
        }

        /// <summary>
        /// Subscribes to events of the specified type, bound to the given lifetime.
        /// </summary>
        /// <param name="action">The event handler callback.</param>
        /// <param name="lifetime">The lifetime boundary; the subscription is automatically cancelled when the lifetime ends.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <returns>A subscription handle.</returns>
        public ISubscription Subscribe<TEvent>(Action<TEvent> action, ILifetime lifetime)
            where TEvent : IEvent {
            ThrowHelper.ThrowIfNull(lifetime, nameof(lifetime));
            return SubscribeInternal(action, 0, lifetime);
        }

        /// <summary>
        /// Subscribes to events of the specified type with explicit priority.
        /// Higher priority subscribers are invoked first during Publish.
        /// </summary>
        /// <param name="action">The event handler callback.</param>
        /// <param name="priority">The dispatch priority. Higher values are invoked first.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <returns>A subscription handle.</returns>
        public ISubscription Subscribe<TEvent>(Action<TEvent> action, int priority)
            where TEvent : IEvent {
            return SubscribeInternal(action, priority, null);
        }

        /// <summary>
        /// Subscribes to events of the specified type with explicit priority and lifetime binding.
        /// </summary>
        /// <param name="action">The event handler callback.</param>
        /// <param name="priority">The dispatch priority. Higher values are invoked first.</param>
        /// <param name="lifetime">The lifetime boundary; the subscription is automatically cancelled when the lifetime ends.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        /// <returns>A subscription handle.</returns>
        public ISubscription Subscribe<TEvent>(Action<TEvent> action, int priority, ILifetime lifetime)
            where TEvent : IEvent {
            ThrowHelper.ThrowIfNull(lifetime, nameof(lifetime));
            return SubscribeInternal(action, priority, lifetime);
        }

        /// <summary>
        /// Cancels the specified event subscription.
        /// </summary>
        /// <param name="subscription">The subscription handle to cancel.</param>
        public void Unsubscribe(ISubscription subscription) {
            if (subscription == null || subscription.Destroyed) {
                return;
            }
            subscription.Destroy();
        }

        /// <summary>
        /// Publishes an event of the specified type, notifying all subscribers in priority order.
        /// Interceptors are invoked before and after dispatch.
        /// </summary>
        /// <param name="payload">The event payload.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        public void Publish<TEvent>(in TEvent payload)
            where TEvent : IEvent {
            ThrowIfNotCreatedOrDestroyed();

            var eventType = typeof(TEvent);

            // Run pre-publish interceptors
            if (_interceptors != null && _interceptors.Count > 0) {
                IEvent eventData = payload;
                for (var i = 0; i < _interceptors.Count; i++) {
                    if (!_interceptors[i].OnBeforePublish(eventType, ref eventData)) {
                        return; // Short-circuited by interceptor
                    }
                }
            }

            if (!_eventSubscriptions.TryGetValue(eventType, out var subscriptions)) {
                return;
            }

            CleanupDestroyedSubscriptions(subscriptions);
            SortIfNeeded(subscriptions, eventType, _dirtyEventTypes);

            var subscriberCount = 0;
            for (var i = 0; i < subscriptions.Count; i++) {
                var subscription = subscriptions[i];
                if (subscription.Destroyed) {
                    continue;
                }

                try {
                    ((Action<TEvent>)subscription.Action).Invoke(payload);
                    subscriberCount++;
                }
                catch (Exception exception) {
                    Logger.Error(LogTag, exception,
                        $"Exception occurred, event type: {eventType.FullName}");
                }
            }

            // Run post-publish interceptors
            if (_interceptors != null && _interceptors.Count > 0) {
                IEvent eventData = payload;
                for (var i = 0; i < _interceptors.Count; i++) {
                    _interceptors[i].OnAfterPublish(eventType, eventData, subscriberCount);
                }
            }
        }

        /// <summary>
        /// Gets the current total number of event subscriptions.
        /// </summary>
        /// <returns>The number of event subscriptions.</returns>
        public int GetEventSubscriptionCount() {
            ThrowIfNotCreatedOrDestroyed();
            var count = 0;
            foreach (var item in _eventSubscriptions) {
                count += item.Value.Count;
            }
            return count;
        }

        #endregion

        #region ICommandDispatcher

        /// <summary>
        /// Registers a handler for the specified command type (default <see cref="RegisterMode.Replace"/>).
        /// </summary>
        /// <param name="handler">The command handler callback.</param>
        /// <typeparam name="TCommand">The command type.</typeparam>
        /// <returns>A subscription handle.</returns>
        public ISubscription Handle<TCommand>(Action<TCommand> handler)
            where TCommand : ICommand {
            return HandleInternal(handler, RegisterMode.Replace, null);
        }

        /// <summary>
        /// Registers a handler for the specified command type with explicit register mode.
        /// </summary>
        /// <param name="handler">The command handler callback.</param>
        /// <param name="mode">The behavior when a handler already exists.</param>
        /// <typeparam name="TCommand">The command type.</typeparam>
        /// <returns>A subscription handle, or <c>null</c> if <see cref="RegisterMode.Ignore"/> and a handler already exists.</returns>
        public ISubscription Handle<TCommand>(Action<TCommand> handler, RegisterMode mode)
            where TCommand : ICommand {
            return HandleInternal(handler, mode, null);
        }

        /// <summary>
        /// Registers a handler for the specified command type, bound to the owner's lifetime.
        /// </summary>
        /// <param name="handler">The command handler callback.</param>
        /// <param name="owner">The subscription owner; the subscription is automatically cancelled when the owner is destroyed.</param>
        /// <typeparam name="TCommand">The command type.</typeparam>
        /// <returns>A subscription handle.</returns>
        public ISubscription Handle<TCommand>(Action<TCommand> handler, BaseObject owner)
            where TCommand : ICommand {
            ThrowHelper.ThrowIfNull(owner, nameof(owner));
            var subscription = HandleInternal(handler, RegisterMode.Replace, null);
            owner.OwnLifetime(subscription);
            return subscription;
        }

        /// <summary>
        /// Registers a handler for the specified command type, bound to the given lifetime.
        /// </summary>
        /// <param name="handler">The command handler callback.</param>
        /// <param name="lifetime">The lifetime boundary; the subscription is automatically cancelled when the lifetime ends.</param>
        /// <typeparam name="TCommand">The command type.</typeparam>
        /// <returns>A subscription handle.</returns>
        public ISubscription Handle<TCommand>(Action<TCommand> handler, ILifetime lifetime)
            where TCommand : ICommand {
            ThrowHelper.ThrowIfNull(lifetime, nameof(lifetime));
            return HandleInternal(handler, RegisterMode.Replace, lifetime);
        }

        /// <summary>
        /// Cancels the specified command handler subscription.
        /// </summary>
        /// <param name="subscription">The subscription handle to cancel.</param>
        public void Unhandle(ISubscription subscription) {
            if (subscription == null || subscription.Destroyed) {
                return;
            }
            subscription.Destroy();
        }

        /// <summary>
        /// Sends a command of the specified type to be executed by the registered handler.
        /// </summary>
        /// <param name="command">The command payload.</param>
        /// <typeparam name="TCommand">The command type.</typeparam>
        public void Send<TCommand>(in TCommand command)
            where TCommand : ICommand {
            ThrowIfNotCreatedOrDestroyed();

            var type = typeof(TCommand);
            if (!_commandSubscriptions.TryGetValue(type, out var subscription)) {
                Logger.Warning(LogTag, $"No handler registered for command type: {type.FullName}");
                return;
            }

            if (subscription.Destroyed) {
                Logger.Warning(LogTag, $"Handler destroyed for command type: {type.FullName}");
                _commandSubscriptions.Remove(type);
                _subscriptionPool.Return(subscription);
                return;
            }

            try {
                ((Action<TCommand>)subscription.Action).Invoke(command);
            }
            catch (Exception exception) {
                Logger.Error(LogTag, exception,
                    $"Exception occurred, command type: {typeof(TCommand).FullName}");
            }
        }

        /// <summary>
        /// Gets the current number of command subscriptions.
        /// </summary>
        /// <returns>The number of command subscriptions.</returns>
        public int GetCommandSubscriptionCount() {
            ThrowIfNotCreatedOrDestroyed();
            var count = 0;
            foreach (var item in _commandSubscriptions) {
                if (!item.Value.Destroyed) {
                    count++;
                }
            }
            return count;
        }

        #endregion

        #region IRequestDispatcher

        /// <summary>
        /// Sends a request and returns the response.
        /// </summary>
        /// <param name="payload">The request payload.</param>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <returns>The response from the handler; default value when no handler is registered.</returns>
        public TResponse Request<TRequest, TResponse>(in TRequest payload)
            where TRequest : IRequest<TResponse> {
            ThrowIfNotCreatedOrDestroyed();

            var type = typeof(TRequest);
            if (!_requestSubscriptions.TryGetValue(type, out var subscription)) {
                Logger.Warning(LogTag, $"No handler registered for request type: {type.FullName}");
                return default;
            }

            if (subscription.Destroyed) {
                Logger.Warning(LogTag, $"Handler destroyed for request type: {type.FullName}");
                _requestSubscriptions.Remove(type);
                _subscriptionPool.Return(subscription);
                return default;
            }

            try {
                return ((Func<TRequest, TResponse>)subscription.Action).Invoke(payload);
            }
            catch (Exception exception) {
                Logger.Error(LogTag, exception,
                    $"Exception occurred, request type: {typeof(TRequest).FullName}");
                return default;
            }
        }

        /// <summary>
        /// Tries to send a request and retrieve the response.
        /// </summary>
        /// <param name="payload">The request payload.</param>
        /// <param name="response">The output response value.</param>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <returns><c>true</c> if a handler exists and was invoked successfully; otherwise <c>false</c>.</returns>
        public bool TryRequest<TRequest, TResponse>(in TRequest payload, out TResponse response)
            where TRequest : IRequest<TResponse> {
            ThrowIfNotCreatedOrDestroyed();
            response = default;

            var type = typeof(TRequest);
            if (!_requestSubscriptions.TryGetValue(type, out var subscription)) {
                return false;
            }

            if (subscription.Destroyed) {
                _requestSubscriptions.Remove(type);
                _subscriptionPool.Return(subscription);
                return false;
            }

            try {
                response = ((Func<TRequest, TResponse>)subscription.Action).Invoke(payload);
                return true;
            }
            catch (Exception exception) {
                Logger.Error(LogTag, exception,
                    $"Exception occurred, request type: {typeof(TRequest).FullName}");
                return false;
            }
        }

        /// <summary>
        /// Registers a handler for the specified request type (default <see cref="RegisterMode.Replace"/>).
        /// </summary>
        /// <param name="handler">The request handler callback.</param>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <returns>A subscription handle.</returns>
        public ISubscription HandleRequest<TRequest, TResponse>(Func<TRequest, TResponse> handler)
            where TRequest : IRequest<TResponse> {
            return HandleRequestInternal<TRequest, TResponse>(handler, RegisterMode.Replace, null);
        }

        /// <summary>
        /// Registers a handler for the specified request type with explicit register mode.
        /// </summary>
        /// <param name="handler">The request handler callback.</param>
        /// <param name="mode">The behavior when a handler already exists.</param>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <returns>A subscription handle, or <c>null</c> if <see cref="RegisterMode.Ignore"/> and a handler already exists.</returns>
        public ISubscription HandleRequest<TRequest, TResponse>(Func<TRequest, TResponse> handler, RegisterMode mode)
            where TRequest : IRequest<TResponse> {
            return HandleRequestInternal<TRequest, TResponse>(handler, mode, null);
        }

        /// <summary>
        /// Registers a handler for the specified request type, bound to the owner's lifetime.
        /// </summary>
        /// <param name="handler">The request handler callback.</param>
        /// <param name="owner">The subscription owner; the subscription is automatically cancelled when the owner is destroyed.</param>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <returns>A subscription handle.</returns>
        public ISubscription HandleRequest<TRequest, TResponse>(Func<TRequest, TResponse> handler, BaseObject owner)
            where TRequest : IRequest<TResponse> {
            ThrowHelper.ThrowIfNull(owner, nameof(owner));
            var subscription = HandleRequestInternal<TRequest, TResponse>(handler, RegisterMode.Replace, null);
            owner.OwnLifetime(subscription);
            return subscription;
        }

        /// <summary>
        /// Registers a handler for the specified request type, bound to the given lifetime.
        /// </summary>
        /// <param name="handler">The request handler callback.</param>
        /// <param name="lifetime">The lifetime boundary; the subscription is automatically cancelled when the lifetime ends.</param>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <returns>A subscription handle.</returns>
        public ISubscription HandleRequest<TRequest, TResponse>(Func<TRequest, TResponse> handler, ILifetime lifetime)
            where TRequest : IRequest<TResponse> {
            ThrowHelper.ThrowIfNull(lifetime, nameof(lifetime));
            return HandleRequestInternal<TRequest, TResponse>(handler, RegisterMode.Replace, lifetime);
        }

        /// <summary>
        /// Cancels the specified request handler subscription.
        /// </summary>
        /// <param name="subscription">The subscription handle to cancel.</param>
        public void UnhandleRequest(ISubscription subscription) {
            if (subscription == null || subscription.Destroyed) {
                return;
            }
            subscription.Destroy();
        }

        /// <summary>
        /// Gets the current number of request subscriptions.
        /// </summary>
        /// <returns>The number of request subscriptions.</returns>
        public int GetRequestSubscriptionCount() {
            ThrowIfNotCreatedOrDestroyed();
            var count = 0;
            foreach (var item in _requestSubscriptions) {
                if (!item.Value.Destroyed) {
                    count++;
                }
            }
            return count;
        }

        #endregion

        #region IDecisionDispatcher

        /// <summary>
        /// Listens for decisions of the specified type with default priority (0).
        /// </summary>
        /// <param name="handler">The decision handler callback; returns <c>true</c> to approve, <c>false</c> to veto.</param>
        /// <typeparam name="TDecision">The decision type.</typeparam>
        /// <returns>A subscription handle.</returns>
        public ISubscription Listen<TDecision>(Func<TDecision, bool> handler)
            where TDecision : IDecision {
            return ListenInternal(handler, 0, null);
        }

        /// <summary>
        /// Listens for decisions of the specified type, bound to the owner's lifetime.
        /// </summary>
        /// <param name="handler">The decision handler callback; returns <c>true</c> to approve, <c>false</c> to veto.</param>
        /// <param name="owner">The subscription owner; the subscription is automatically cancelled when the owner is destroyed.</param>
        /// <typeparam name="TDecision">The decision type.</typeparam>
        /// <returns>A subscription handle.</returns>
        public ISubscription Listen<TDecision>(Func<TDecision, bool> handler, BaseObject owner)
            where TDecision : IDecision {
            ThrowHelper.ThrowIfNull(owner, nameof(owner));
            var subscription = ListenInternal(handler, 0, null);
            owner.OwnLifetime(subscription);
            return subscription;
        }

        /// <summary>
        /// Listens for decisions of the specified type, bound to the given lifetime.
        /// </summary>
        /// <param name="handler">The decision handler callback; returns <c>true</c> to approve, <c>false</c> to veto.</param>
        /// <param name="lifetime">The lifetime boundary; the subscription is automatically cancelled when the lifetime ends.</param>
        /// <typeparam name="TDecision">The decision type.</typeparam>
        /// <returns>A subscription handle.</returns>
        public ISubscription Listen<TDecision>(Func<TDecision, bool> handler, ILifetime lifetime)
            where TDecision : IDecision {
            ThrowHelper.ThrowIfNull(lifetime, nameof(lifetime));
            return ListenInternal(handler, 0, lifetime);
        }

        /// <summary>
        /// Listens for decisions of the specified type with explicit priority.
        /// Higher priority listeners are invoked first.
        /// </summary>
        /// <param name="handler">The decision handler callback; returns <c>true</c> to approve, <c>false</c> to veto.</param>
        /// <param name="priority">The dispatch priority. Higher values are invoked first.</param>
        /// <typeparam name="TDecision">The decision type.</typeparam>
        /// <returns>A subscription handle.</returns>
        public ISubscription Listen<TDecision>(Func<TDecision, bool> handler, int priority)
            where TDecision : IDecision {
            return ListenInternal(handler, priority, null);
        }

        /// <summary>
        /// Listens for decisions of the specified type with explicit priority and lifetime binding.
        /// </summary>
        /// <param name="handler">The decision handler callback; returns <c>true</c> to approve, <c>false</c> to veto.</param>
        /// <param name="priority">The dispatch priority. Higher values are invoked first.</param>
        /// <param name="lifetime">The lifetime boundary; the subscription is automatically cancelled when the lifetime ends.</param>
        /// <typeparam name="TDecision">The decision type.</typeparam>
        /// <returns>A subscription handle.</returns>
        public ISubscription Listen<TDecision>(Func<TDecision, bool> handler, int priority, ILifetime lifetime)
            where TDecision : IDecision {
            ThrowHelper.ThrowIfNull(lifetime, nameof(lifetime));
            return ListenInternal(handler, priority, lifetime);
        }

        /// <summary>
        /// Cancels the specified decision listener subscription.
        /// </summary>
        /// <param name="subscription">The subscription handle to cancel.</param>
        public void Unlisten(ISubscription subscription) {
            if (subscription == null || subscription.Destroyed) {
                return;
            }
            subscription.Destroy();
        }

        /// <summary>
        /// Initiates a decision vote; all listeners must approve for the result to be <c>true</c>.
        /// Listeners are invoked in priority order; the first veto short-circuits.
        /// </summary>
        /// <param name="decision">The decision payload.</param>
        /// <typeparam name="TDecision">The decision type.</typeparam>
        /// <returns><c>true</c> if all listeners approved; <c>false</c> if no listeners exist or any listener vetoed.</returns>
        public bool Decide<TDecision>(in TDecision decision)
            where TDecision : IDecision {
            ThrowIfNotCreatedOrDestroyed();

            if (!_decisionSubscriptions.TryGetValue(typeof(TDecision), out var subscriptions)) {
                return true;
            }

            CleanupDestroyedSubscriptions(subscriptions);
            SortIfNeeded(subscriptions, typeof(TDecision), _dirtyDecisionTypes);

            var pass = true;
            for (var i = 0; i < subscriptions.Count; i++) {
                var subscription = subscriptions[i];
                if (subscription.Destroyed) {
                    continue;
                }

                try {
                    if (!((Func<TDecision, bool>)subscription.Action).Invoke(decision)) {
                        pass = false;
                        break;
                    }
                }
                catch (Exception exception) {
                    Logger.Error(LogTag, exception,
                        $"Exception occurred, decision type: {typeof(TDecision).FullName}");
                }
            }

            return pass;
        }

        /// <summary>
        /// Gets the current number of decision subscriptions.
        /// </summary>
        /// <returns>The number of decision subscriptions.</returns>
        public int GetDecisionSubscriptionCount() {
            ThrowIfNotCreatedOrDestroyed();
            var count = 0;
            foreach (var kv in _decisionSubscriptions) {
                count += kv.Value.Count;
            }
            return count;
        }

        #endregion

        #region IDispatcher

        /// <summary>
        /// Removes all registered subscriptions.
        /// </summary>
        public void RemoveAllSubscriptions() {
            ThrowIfNotCreatedOrDestroyed();
            ClearListSubscriptions(_eventSubscriptions);
            ClearSingleSubscriptions(_commandSubscriptions);
            ClearSingleSubscriptions(_requestSubscriptions);
            ClearListSubscriptions(_decisionSubscriptions);
        }

        /// <summary>
        /// Gets the total number of subscriptions across all types.
        /// </summary>
        /// <returns>The total subscription count.</returns>
        public int GetTotalSubscriptionCount() {
            ThrowIfNotCreatedOrDestroyed();
            return GetEventSubscriptionCount()
                   + GetCommandSubscriptionCount()
                   + GetRequestSubscriptionCount()
                   + GetDecisionSubscriptionCount();
        }

        /// <summary>
        /// Gets a diagnostics snapshot containing subscription counts for all dispatching categories.
        /// </summary>
        /// <returns>A diagnostics snapshot with current subscription counts.</returns>
        public DiagnosticsSnapshot GetDiagnostics() {
            ThrowIfNotCreatedOrDestroyed();
            return new DiagnosticsSnapshot(
                GetEventSubscriptionCount(),
                GetCommandSubscriptionCount(),
                GetRequestSubscriptionCount(),
                GetDecisionSubscriptionCount());
        }

        #endregion

        #region Interceptor Management

        /// <summary>
        /// Adds an event interceptor to the pipeline. Interceptors are invoked in registration order.
        /// </summary>
        /// <param name="interceptor">The interceptor to add.</param>
        public void AddInterceptor(IEventInterceptor interceptor) {
            ThrowHelper.ThrowIfNull(interceptor, nameof(interceptor));
            ThrowIfNotCreatedOrDestroyed();
            if (_interceptors == null) {
                _interceptors = new List<IEventInterceptor>(4);
            }
            _interceptors.Add(interceptor);
        }

        /// <summary>
        /// Removes an event interceptor from the pipeline.
        /// </summary>
        /// <param name="interceptor">The interceptor to remove.</param>
        /// <returns><c>true</c> if the interceptor was found and removed; otherwise <c>false</c>.</returns>
        public bool RemoveInterceptor(IEventInterceptor interceptor) {
            if (_interceptors == null || interceptor == null) {
                return false;
            }
            return _interceptors.Remove(interceptor);
        }

        /// <summary>
        /// Gets the current number of registered interceptors.
        /// </summary>
        /// <returns>The interceptor count.</returns>
        public int GetInterceptorCount() {
            return _interceptors?.Count ?? 0;
        }

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initializes subscription storage, the subscription object pool, and dirty-tracking sets.
        /// </summary>
        protected override void OnCreate() {
            _index = 1;
            _registrationOrder = 0;
            _eventSubscriptions = new Dictionary<Type, List<Subscription>>();
            _commandSubscriptions = new Dictionary<Type, Subscription>();
            _requestSubscriptions = new Dictionary<Type, Subscription>();
            _decisionSubscriptions = new Dictionary<Type, List<Subscription>>();
            _dirtyEventTypes = new HashSet<Type>();
            _dirtyDecisionTypes = new HashSet<Type>();

            _subscriptionPool = new SubscriptionPool();
            _subscriptionPool.Create();
        }

        /// <summary>
        /// Destroys all subscriptions, interceptors, and releases the subscription object pool.
        /// </summary>
        protected override void OnDestroy() {
            ClearListSubscriptions(_eventSubscriptions);
            ClearSingleSubscriptions(_commandSubscriptions);
            ClearSingleSubscriptions(_requestSubscriptions);
            ClearListSubscriptions(_decisionSubscriptions);

            _subscriptionPool?.Destroy();

            _eventSubscriptions = null;
            _commandSubscriptions = null;
            _requestSubscriptions = null;
            _decisionSubscriptions = null;
            _subscriptionPool = null;
            _interceptors = null;
            _dirtyEventTypes = null;
            _dirtyDecisionTypes = null;
        }

        #endregion

        #region Internal: Subscribe helpers

        private ISubscription SubscribeInternal<TEvent>(Action<TEvent> action, int priority, ILifetime lifetime)
            where TEvent : IEvent {
            ThrowHelper.ThrowIfNull(action, nameof(action));

            var subscription = _subscriptionPool.Get();
            subscription.Handle = _index++;
            subscription.MessageType = typeof(TEvent);
            subscription.Action = action;
            subscription.Priority = priority;
            subscription.RegistrationOrder = _registrationOrder++;

            if (!_eventSubscriptions.TryGetValue(typeof(TEvent), out var subscriptions)) {
                subscriptions = _eventSubscriptions[typeof(TEvent)] = new List<Subscription>();
            }

            subscriptions.Add(subscription);
            _dirtyEventTypes.Add(typeof(TEvent));
            lifetime?.Add(subscription);
            return subscription;
        }

        private Subscription HandleInternal<TCommand>(Action<TCommand> handler, RegisterMode mode, ILifetime lifetime)
            where TCommand : ICommand {
            ThrowHelper.ThrowIfNull(handler, nameof(handler));

            var type = typeof(TCommand);
            if (_commandSubscriptions.TryGetValue(type, out var existing) && !existing.Destroyed) {
                switch (mode) {
                    case RegisterMode.Throw:
                        ThrowHelper.ThrowArgumentException(
                            $"Handler already registered for command type: {type.FullName}");
                        return null;
                    case RegisterMode.Ignore:
                        return null;
                    case RegisterMode.Replace:
                        Logger.Warning(LogTag, $"Replacing existing handler for command type: {type.FullName}");
                        existing.Destroy();
                        _subscriptionPool.Return(existing);
                        break;
                }
            }

            var subscription = _subscriptionPool.Get();
            subscription.Handle = _index++;
            subscription.MessageType = type;
            subscription.Action = handler;

            _commandSubscriptions[type] = subscription;
            lifetime?.Add(subscription);
            return subscription;
        }

        private Subscription HandleRequestInternal<TRequest, TResponse>(Func<TRequest, TResponse> handler, RegisterMode mode, ILifetime lifetime)
            where TRequest : IRequest<TResponse> {
            ThrowHelper.ThrowIfNull(handler, nameof(handler));

            var type = typeof(TRequest);
            if (_requestSubscriptions.TryGetValue(type, out var existing) && !existing.Destroyed) {
                switch (mode) {
                    case RegisterMode.Throw:
                        ThrowHelper.ThrowArgumentException(
                            $"Handler already registered for request type: {type.FullName}");
                        return null;
                    case RegisterMode.Ignore:
                        return null;
                    case RegisterMode.Replace:
                        Logger.Warning(LogTag, $"Replacing existing handler for request type: {type.FullName}");
                        existing.Destroy();
                        _subscriptionPool.Return(existing);
                        break;
                }
            }

            var subscription = _subscriptionPool.Get();
            subscription.Handle = _index++;
            subscription.MessageType = type;
            subscription.Action = handler;

            _requestSubscriptions[type] = subscription;
            lifetime?.Add(subscription);
            return subscription;
        }

        private ISubscription ListenInternal<TDecision>(Func<TDecision, bool> handler, int priority, ILifetime lifetime)
            where TDecision : IDecision {
            ThrowHelper.ThrowIfNull(handler, nameof(handler));

            var subscription = _subscriptionPool.Get();
            subscription.Handle = _index++;
            subscription.MessageType = typeof(TDecision);
            subscription.Action = handler;
            subscription.Priority = priority;
            subscription.RegistrationOrder = _registrationOrder++;

            if (!_decisionSubscriptions.TryGetValue(typeof(TDecision), out var subscriptions)) {
                subscriptions = _decisionSubscriptions[typeof(TDecision)] = new List<Subscription>();
            }

            subscriptions.Add(subscription);
            _dirtyDecisionTypes.Add(typeof(TDecision));
            lifetime?.Add(subscription);
            return subscription;
        }

        #endregion

        #region Internal: Priority sorting

        private static void SortIfNeeded(List<Subscription> subscriptions, Type type, HashSet<Type> dirtySet) {
            if (dirtySet == null || !dirtySet.Remove(type)) {
                return;
            }

            // Sort by priority descending, then by registration order ascending for stable ordering
            subscriptions.Sort((a, b) => {
                var priorityDiff = b.Priority.CompareTo(a.Priority);
                return priorityDiff != 0 ? priorityDiff : a.RegistrationOrder.CompareTo(b.RegistrationOrder);
            });
        }

        #endregion

        #region Internal: Cleanup

        private void CleanupDestroyedSubscriptions(List<Subscription> subscriptions) {
            for (var i = subscriptions.Count - 1; i >= 0; i--) {
                if (subscriptions[i].Destroyed) {
                    _subscriptionPool.Return(subscriptions[i]);
                    subscriptions.RemoveAt(i);
                }
            }
        }

        private void ClearListSubscriptions(Dictionary<Type, List<Subscription>> map) {
            if (map == null) {
                return;
            }

            foreach (var pair in map) {
                var subscriptions = pair.Value;
                if (subscriptions == null) {
                    continue;
                }

                for (var i = subscriptions.Count - 1; i >= 0; i--) {
                    var subscription = subscriptions[i];
                    if (subscription == null) {
                        continue;
                    }

                    if (!subscription.Destroyed) {
                        subscription.Destroy();
                    }
                    _subscriptionPool.Return(subscription);
                }

                subscriptions.Clear();
            }

            map.Clear();
        }

        private void ClearSingleSubscriptions(Dictionary<Type, Subscription> map) {
            if (map == null) {
                return;
            }

            foreach (var pair in map) {
                var subscription = pair.Value;
                if (subscription == null) {
                    continue;
                }

                if (!subscription.Destroyed) {
                    subscription.Destroy();
                }
                _subscriptionPool.Return(subscription);
            }

            map.Clear();
        }

        #endregion
    }
}