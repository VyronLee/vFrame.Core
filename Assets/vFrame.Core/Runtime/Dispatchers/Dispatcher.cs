using System;
using System.Collections.Generic;
using vFrame.Core.Base;
using vFrame.Core.Containers;
using vFrame.Core.Exceptions;
using vFrame.Core.Loggers;

namespace vFrame.Core.Dispatchers
{
    public class Dispatcher : Component, IDispatcher
    {
        public readonly struct DiagnosticsSnapshot
        {
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

        private uint _index = 1;
        private Dictionary<Type, List<Subscription>> _eventSubscriptions;
        private Dictionary<Type, Subscription> _commandSubscriptions;
        private Dictionary<Type, Subscription> _requestSubscriptions;
        private Dictionary<Type, List<Subscription>> _decisionSubscriptions;
        private SubscriptionPool _subscriptionPool;

        #region IEventDispatcher

        public ISubscription Subscribe<TEvent>(Action<TEvent> action)
            where TEvent : IEvent {
            return SubscribeEventInternal(action, null);
        }

        public ISubscription Subscribe<TEvent>(Action<TEvent> action, BaseObject owner)
            where TEvent : IEvent {
            ThrowHelper.ThrowIfNull(owner, nameof(owner));
            var subscription = SubscribeEventInternal(action, null);
            owner.OwnLifetime(subscription);
            return subscription;
        }

        public ISubscription Subscribe<TEvent>(Action<TEvent> action, ILifetime lifetime)
            where TEvent : IEvent {
            ThrowHelper.ThrowIfNull(lifetime, nameof(lifetime));
            return SubscribeEventInternal(action, lifetime);
        }

        public void Unsubscribe(ISubscription subscription) {
            if (subscription == null || subscription.Destroyed) {
                return;
            }
            subscription.Destroy();
        }

        public void Publish<TEvent>(in TEvent payload)
            where TEvent : IEvent {
            ThrowIfNotCreatedOrDestroyed();

            if (!_eventSubscriptions.TryGetValue(typeof(TEvent), out var subscriptions)) {
                return;
            }

            CleanupDestroyedSubscriptions(subscriptions);

            for (var i = 0; i < subscriptions.Count; i++) {
                var subscription = subscriptions[i];
                if (subscription.Destroyed) {
                    continue;
                }

                try {
                    ((Action<TEvent>)subscription.Action).Invoke(payload);
                }
                catch (Exception exception) {
                    Logger.Error(LogTag, "Exception occurred, event type: {0}, exception: {1}",
                        typeof(TEvent).FullName, exception);
                }
            }
        }

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

        public ISubscription Handle<TCommand>(Action<TCommand> handler)
            where TCommand : ICommand {
            return HandleCommandInternal(handler, null);
        }

        public ISubscription Handle<TCommand>(Action<TCommand> handler, BaseObject owner)
            where TCommand : ICommand {
            ThrowHelper.ThrowIfNull(owner, nameof(owner));
            var subscription = HandleCommandInternal(handler, null);
            owner.OwnLifetime(subscription);
            return subscription;
        }

        public ISubscription Handle<TCommand>(Action<TCommand> handler, ILifetime lifetime)
            where TCommand : ICommand {
            ThrowHelper.ThrowIfNull(lifetime, nameof(lifetime));
            return HandleCommandInternal(handler, lifetime);
        }

        public void Unhandle(ISubscription subscription) {
            if (subscription == null || subscription.Destroyed) {
                return;
            }
            subscription.Destroy();
        }

        public void Send<TCommand>(in TCommand command)
            where TCommand : ICommand {
            ThrowIfNotCreatedOrDestroyed();

            var type = typeof(TCommand);
            if (!_commandSubscriptions.TryGetValue(type, out var subscription)) {
                Logger.Warning(LogTag, "No handler registered for command type: {0}",
                    type.FullName);
                return;
            }

            if (subscription.Destroyed) {
                Logger.Warning(LogTag, "Handler destroyed for command type: {0}",
                    type.FullName);
                _commandSubscriptions.Remove(type);
                _subscriptionPool.Return(subscription);
                return;
            }

            try {
                ((Action<TCommand>)subscription.Action).Invoke(command);
            }
            catch (Exception exception) {
                Logger.Error(LogTag, "Exception occurred, command type: {0}, exception: {1}",
                    typeof(TCommand).FullName, exception);
            }
        }

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

        public TResponse Request<TRequest, TResponse>(in TRequest payload)
            where TRequest : IRequest<TResponse> {
            ThrowIfNotCreatedOrDestroyed();

            var type = typeof(TRequest);
            if (!_requestSubscriptions.TryGetValue(type, out var subscription)) {
                Logger.Warning(LogTag, "No handler registered for request type: {0}",
                    type.FullName);
                return default;
            }

            if (subscription.Destroyed) {
                Logger.Warning(LogTag, "Handler destroyed for request type: {0}",
                    type.FullName);
                _requestSubscriptions.Remove(type);
                _subscriptionPool.Return(subscription);
                return default;
            }

            try {
                return ((Func<TRequest, TResponse>)subscription.Action).Invoke(payload);
            }
            catch (Exception exception) {
                Logger.Error(LogTag, "Exception occurred, request type: {0}, exception: {1}",
                    type.FullName, exception);
                return default;
            }
        }

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
                Logger.Error(LogTag, "Exception occurred, request type: {0}, exception: {1}",
                    type.FullName, exception);
                return false;
            }
        }

        public ISubscription HandleRequest<TRequest, TResponse>(Func<TRequest, TResponse> handler)
            where TRequest : IRequest<TResponse> {
            return HandleRequestInternal<TRequest, TResponse>(handler, null);
        }

        public ISubscription HandleRequest<TRequest, TResponse>(Func<TRequest, TResponse> handler, BaseObject owner)
            where TRequest : IRequest<TResponse> {
            ThrowHelper.ThrowIfNull(owner, nameof(owner));
            var subscription = HandleRequestInternal<TRequest, TResponse>(handler, null);
            owner.OwnLifetime(subscription);
            return subscription;
        }

        public ISubscription HandleRequest<TRequest, TResponse>(Func<TRequest, TResponse> handler, ILifetime lifetime)
            where TRequest : IRequest<TResponse> {
            ThrowHelper.ThrowIfNull(lifetime, nameof(lifetime));
            return HandleRequestInternal<TRequest, TResponse>(handler, lifetime);
        }

        public void UnhandleRequest(ISubscription subscription) {
            if (subscription == null || subscription.Destroyed) {
                return;
            }
            subscription.Destroy();
        }

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

        public ISubscription Listen<TDecision>(Func<TDecision, bool> handler)
            where TDecision : IDecision {
            return ListenDecisionInternal(handler, null);
        }

        public ISubscription Listen<TDecision>(Func<TDecision, bool> handler, BaseObject owner)
            where TDecision : IDecision {
            ThrowHelper.ThrowIfNull(owner, nameof(owner));
            var subscription = ListenDecisionInternal(handler, null);
            owner.OwnLifetime(subscription);
            return subscription;
        }

        public ISubscription Listen<TDecision>(Func<TDecision, bool> handler, ILifetime lifetime)
            where TDecision : IDecision {
            ThrowHelper.ThrowIfNull(lifetime, nameof(lifetime));
            return ListenDecisionInternal(handler, lifetime);
        }

        public void Unlisten(ISubscription subscription) {
            if (subscription == null || subscription.Destroyed) {
                return;
            }
            subscription.Destroy();
        }

        public bool Decide<TDecision>(in TDecision decision)
            where TDecision : IDecision {
            ThrowIfNotCreatedOrDestroyed();

            if (!_decisionSubscriptions.TryGetValue(typeof(TDecision), out var subscriptions)) {
                return true;
            }

            CleanupDestroyedSubscriptions(subscriptions);

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
                    Logger.Error(LogTag, "Exception occurred, decision type: {0}, exception: {1}",
                        typeof(TDecision).FullName, exception);
                }
            }

            return pass;
        }

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

        public void RemoveAllSubscriptions() {
            ThrowIfNotCreatedOrDestroyed();
            ClearListSubscriptions(_eventSubscriptions);
            ClearSingleSubscriptions(_commandSubscriptions);
            ClearSingleSubscriptions(_requestSubscriptions);
            ClearListSubscriptions(_decisionSubscriptions);
        }

        public int GetTotalSubscriptionCount() {
            ThrowIfNotCreatedOrDestroyed();
            return GetEventSubscriptionCount()
                   + GetCommandSubscriptionCount()
                   + GetRequestSubscriptionCount()
                   + GetDecisionSubscriptionCount();
        }

        public DiagnosticsSnapshot GetDiagnostics() {
            ThrowIfNotCreatedOrDestroyed();
            return new DiagnosticsSnapshot(
                GetEventSubscriptionCount(),
                GetCommandSubscriptionCount(),
                GetRequestSubscriptionCount(),
                GetDecisionSubscriptionCount());
        }

        #endregion

        #region Lifecycle

        protected override void OnCreate() {
            _eventSubscriptions = new Dictionary<Type, List<Subscription>>();
            _commandSubscriptions = new Dictionary<Type, Subscription>();
            _requestSubscriptions = new Dictionary<Type, Subscription>();
            _decisionSubscriptions = new Dictionary<Type, List<Subscription>>();

            _subscriptionPool = new SubscriptionPool();
            _subscriptionPool.Create();
        }

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
        }

        #endregion

        #region Internal: Subscribe helpers

        private Subscription SubscribeEventInternal<TEvent>(Action<TEvent> action, ILifetime lifetime)
            where TEvent : IEvent {
            ThrowHelper.ThrowIfNull(action, nameof(action));

            var subscription = _subscriptionPool.Get();
            subscription.Handle = _index++;
            subscription.MessageType = typeof(TEvent);
            subscription.Action = action;

            if (!_eventSubscriptions.TryGetValue(typeof(TEvent), out var subscriptions)) {
                subscriptions = _eventSubscriptions[typeof(TEvent)] = new List<Subscription>();
            }

            subscriptions.Add(subscription);
            lifetime?.Add(subscription);
            return subscription;
        }

        private Subscription HandleCommandInternal<TCommand>(Action<TCommand> handler, ILifetime lifetime)
            where TCommand : ICommand {
            ThrowHelper.ThrowIfNull(handler, nameof(handler));

            var type = typeof(TCommand);
            if (_commandSubscriptions.TryGetValue(type, out var existing) && !existing.Destroyed) {
                Logger.Warning(LogTag, "Replacing existing handler for command type: {0}",
                    type.FullName);
                existing.Destroy();
                _subscriptionPool.Return(existing);
            }

            var subscription = _subscriptionPool.Get();
            subscription.Handle = _index++;
            subscription.MessageType = type;
            subscription.Action = handler;

            _commandSubscriptions[type] = subscription;
            lifetime?.Add(subscription);
            return subscription;
        }

        private Subscription HandleRequestInternal<TRequest, TResponse>(Func<TRequest, TResponse> handler, ILifetime lifetime)
            where TRequest : IRequest<TResponse> {
            ThrowHelper.ThrowIfNull(handler, nameof(handler));

            var type = typeof(TRequest);
            if (_requestSubscriptions.TryGetValue(type, out var existing) && !existing.Destroyed) {
                Logger.Warning(LogTag, "Replacing existing handler for request type: {0}",
                    type.FullName);
                existing.Destroy();
                _subscriptionPool.Return(existing);
            }

            var subscription = _subscriptionPool.Get();
            subscription.Handle = _index++;
            subscription.MessageType = type;
            subscription.Action = handler;

            _requestSubscriptions[type] = subscription;
            lifetime?.Add(subscription);
            return subscription;
        }

        private Subscription ListenDecisionInternal<TDecision>(Func<TDecision, bool> handler, ILifetime lifetime)
            where TDecision : IDecision {
            ThrowHelper.ThrowIfNull(handler, nameof(handler));

            var subscription = _subscriptionPool.Get();
            subscription.Handle = _index++;
            subscription.MessageType = typeof(TDecision);
            subscription.Action = handler;

            if (!_decisionSubscriptions.TryGetValue(typeof(TDecision), out var subscriptions)) {
                subscriptions = _decisionSubscriptions[typeof(TDecision)] = new List<Subscription>();
            }

            subscriptions.Add(subscription);
            lifetime?.Add(subscription);
            return subscription;
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
