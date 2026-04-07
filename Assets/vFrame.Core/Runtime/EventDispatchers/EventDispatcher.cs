using System;
using System.Collections.Generic;
using vFrame.Core.Containers;
using vFrame.Core.Exceptions;
using vFrame.Core.Loggers;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.EventDispatchers
{
    public class EventDispatcher : Component, IEventDispatcher
    {
<<<<<<< Updated upstream
        private static readonly LogTag EventLogTag = new LogTag("EventDispatcher");

        private Dictionary<int, List<EventExecutor>> _eventExecutorLists;
=======
        public readonly struct DiagnosticsSnapshot
        {
            public DiagnosticsSnapshot(int interactionSubscriptionCount, int decisionSubscriptionCount) {
                InteractionSubscriptionCount = interactionSubscriptionCount;
                DecisionSubscriptionCount = decisionSubscriptionCount;
            }

            public int InteractionSubscriptionCount { get; }

            public int DecisionSubscriptionCount { get; }
        }

        private static readonly LogTag EventLogTag = new LogTag("EventDispatcher");

>>>>>>> Stashed changes
        private uint _index = 1;
        private Dictionary<Type, List<InteractionSubscription>> _subscriptions;
        private Dictionary<Type, List<DecisionSubscription>> _decisionSubscriptions;
        private InteractionSubscriptionPool _subscriptionPool;
        private DecisionSubscriptionPool _decisionPool;

<<<<<<< Updated upstream
        public uint AddEventListener(IEventListener listener, int eventId) {
            ThrowHelper.ThrowIfNull(listener, nameof(listener));

            var executor = EventExecutorPool.Shared.Get();
            executor.EventId = eventId;
            executor.Listener = listener;
            executor.Handle = _index++;

            if (!_eventExecutorLists.TryGetValue(eventId, out var executors)) {
                executors = _eventExecutorLists[eventId] = new List<EventExecutor>();
            }
            executors.Add(executor);

            return executor.Handle;
        }

        public uint AddEventListener(Action<IEvent> action, int eventId) {
            ThrowHelper.ThrowIfNull(action, nameof(action));

            var delegateEventListener = ObjectPool<DelegateEventListener>.Shared.Get();
            delegateEventListener.Action = action;

            return AddEventListener(delegateEventListener, eventId);
        }

        public IEventListener RemoveEventListener(uint handle) {
            IEventListener listener = null;
            foreach (var item in _eventExecutorLists) {
                var list = item.Value;
                foreach (var e in list) {
                    if (e.Handle == handle) {
                        e.Stop();
                        listener = e.Listener;
                    }
                }
            }
            if (listener is DelegateEventListener eventListener) {
                ObjectPool<DelegateEventListener>.Shared.Return(eventListener);
            }
            return listener;
        }

        public void DispatchEvent(int eventId) {
            DispatchEvent(eventId, null);
        }

        public void DispatchEvent(int eventId, object context) {
            if (!_eventExecutorLists.ContainsKey(eventId)) {
                return;
            }

            var executorList = _eventExecutorLists[eventId];

            // 移除已经停止的执行器
            var len = executorList.Count;
            for (var i = len - 1; i >= 0; --i) {
                if (executorList[i].Stopped) {
                    EventExecutorPool.Shared.Return(executorList[i]);
                    executorList.RemoveAt(i);
                }
            }

            // 必须先激活已存在的执行器, 执行过程中新增的执行器不需要生效
            foreach (var executor in executorList) {
                executor.Activate();
            }

            // 循环执行
            var e = EventPool.Shared.Get();
            e.EventId = eventId;
            e.Context = context;
            e.Target = this;

            // 注意这里不能用foreach, 否则事件派发的过程添加同ID的注册会导致异常
            for (var i = 0; i < executorList.Count; i++) {
                var executor = executorList[i];
                if (!executor.Activated || executor.Stopped) {
=======
        public IInteractionSubscription Subscribe<TMessage>(Action<TMessage> action)
            where TMessage : class, IInteractionMessage {
            return SubscribeInternal(action, null);
        }

        public IInteractionSubscription Subscribe<TMessage>(Action<TMessage> action, BaseObject owner)
            where TMessage : class, IInteractionMessage {
            ThrowHelper.ThrowIfNull(owner, nameof(owner));

            var subscription = SubscribeInternal(action, null);
            return BindSubscriptionToOwner(subscription, owner);
        }

        public IInteractionSubscription Subscribe<TMessage>(Action<TMessage> action, ILifetime lifetime)
            where TMessage : class, IInteractionMessage {
            ThrowHelper.ThrowIfNull(lifetime, nameof(lifetime));

            var subscription = SubscribeInternal(action, lifetime);
            return subscription;
        }

        public void Unsubscribe(ISubscription subscription) {
            if (subscription == null || subscription.Destroyed) {
                return;
            }

            subscription.Destroy();
        }

        public void Publish<TMessage>(TMessage message)
            where TMessage : class, IInteractionMessage {
            ThrowIfNotCreatedOrDestroyed();
            ThrowHelper.ThrowIfNull(message, nameof(message));

            if (!_subscriptions.TryGetValue(typeof(TMessage), out var subscriptions)) {
                return;
            }

            CleanupDestroyedSubscriptions(subscriptions);

            for (var i = 0; i < subscriptions.Count; i++) {
                var subscription = subscriptions[i];
                if (subscription.Destroyed) {
>>>>>>> Stashed changes
                    continue;
                }

                try {
<<<<<<< Updated upstream
                    executor.Execute(e);
                }
                catch (Exception exception) {
                    Logger.Error(EventLogTag, "Exception occurred, event id: {0}, exception: {1}",
                        eventId, exception);
                }
            }

            EventPool.Shared.Return(e);
        }

        public uint AddVoteListener(IVoteListener listener, int voteId) {
            ThrowHelper.ThrowIfNull(listener, nameof(listener));

            var executor = VoteExecutorPool.Shared.Get();
            executor.VoteId = voteId;
            executor.Listener = listener;
            executor.Handle = _index++;

            if (!_voteExecutorLists.TryGetValue(voteId, out var executors)) {
                executors = _voteExecutorLists[voteId] = new List<VoteExecutor>();
            }
            executors.Add(executor);

            return executor.Handle;
        }

        public uint AddVoteListener(Func<IVote, bool> func, int voteId) {
            ThrowHelper.ThrowIfNull(func, nameof(func));

            var listener = ObjectPool<DelegateVoteListener>.Shared.Get();
            listener.VoteAction = func;

            return AddVoteListener(listener, voteId);
        }

        public IVoteListener RemoveVoteListener(uint handle) {
            IVoteListener listener = null;
            foreach (var item in _voteExecutorLists) {
                var list = item.Value;
                foreach (var v in list) {
                    if (v.Handle == handle) {
                        v.Stop();
                        listener = v.Listener;
                        break;
                    }
                }
            }
            if (listener is DelegateVoteListener voteListener) {
                ObjectPool<DelegateVoteListener>.Shared.Return(voteListener);
            }
            return listener;
        }

        public bool DispatchVote(int voteId) {
            return DispatchVote(voteId, null);
        }

        public bool DispatchVote(int voteId, object context) {
            if (!_voteExecutorLists.ContainsKey(voteId)) {
                return true;
            }

            var executorList = _voteExecutorLists[voteId];

            // 移除已经停止的执行器
            var len = executorList.Count;
            for (var i = len - 1; i >= 0; --i) {
                if (executorList[i].Stopped) {
                    VoteExecutorPool.Shared.Return(executorList[i]);
                    executorList.RemoveAt(i);
                }
            }

            // 必须先激活已存在的执行器, 执行过程中新增的执行器不需要生效
            foreach (var executor in executorList) {
                executor.Activate();
            }

            // 循环执行
            var e = VotePool.Shared.Get();
            e.VoteId = voteId;
            e.Context = context;
            e.Target = this;

            var pass = true;
            // 注意这里不能用foreach, 否则事件派发的过程添加同ID的注册会导致异常
            for (var i = 0; i < executorList.Count; i++) {
                var executor = executorList[i];
                if (!executor.Activated || executor.Stopped) {
=======
                    ((Action<TMessage>)subscription.Action)?.Invoke(message);
                }
                catch (Exception exception) {
                    Logger.Error(EventLogTag,
                        "Exception occurred, interaction type: {0}, exception: {1}",
                        typeof(TMessage).FullName, exception);
                }
            }
        }

        public IDecisionSubscription Listen<TDecision>(Func<TDecision, bool> handler)
            where TDecision : class, IDecisionMessage {
            return ListenInternal(handler, null);
        }

        public IDecisionSubscription Listen<TDecision>(Func<TDecision, bool> handler, BaseObject owner)
            where TDecision : class, IDecisionMessage {
            ThrowHelper.ThrowIfNull(owner, nameof(owner));

            var subscription = ListenInternal(handler, null);
            return BindDecisionToOwner(subscription, owner);
        }

        public IDecisionSubscription Listen<TDecision>(Func<TDecision, bool> handler, ILifetime lifetime)
            where TDecision : class, IDecisionMessage {
            ThrowHelper.ThrowIfNull(lifetime, nameof(lifetime));

            var subscription = ListenInternal(handler, lifetime);
            return subscription;
        }

        public bool Decide<TDecision>(TDecision decision)
            where TDecision : class, IDecisionMessage {
            ThrowIfNotCreatedOrDestroyed();
            ThrowHelper.ThrowIfNull(decision, nameof(decision));

            if (!_decisionSubscriptions.TryGetValue(typeof(TDecision), out var subscriptions)) {
                return true;
            }

            CleanupDestroyedDecisionSubscriptions(subscriptions);

            var pass = true;
            for (var i = 0; i < subscriptions.Count; i++) {
                var subscription = subscriptions[i];
                if (subscription.Destroyed) {
>>>>>>> Stashed changes
                    continue;
                }

                try {
<<<<<<< Updated upstream
                    if (executor.Execute(e)) {
                        continue;
                    }
                    pass = false;
                    break;
                }
                catch (Exception exception) {
                    Logger.Error(EventLogTag, "Exception occurred, vote id: {0}, exception: {1}",
                        voteId, exception);
                }
            }

            VotePool.Shared.Return(e);
            return pass;
        }

        public void RemoveAllListeners() {
            _eventExecutorLists.Clear();
            _voteExecutorLists.Clear();
        }

        public int GetEventExecutorCount() {
=======
                    if (!((Func<TDecision, bool>)subscription.Handler).Invoke(decision)) {
                        pass = false;
                        break;
                    }
                }
                catch (Exception exception) {
                    Logger.Error(EventLogTag,
                        "Exception occurred, decision type: {0}, exception: {1}",
                        typeof(TDecision).FullName, exception);
                }
            }

            return pass;
        }

        public void RemoveAllSubscriptions() {
            ThrowIfNotCreatedOrDestroyed();
            ClearSubscriptions();
            ClearDecisionSubscriptions();
        }

        public int GetInteractionSubscriptionCount() {
            ThrowIfNotCreatedOrDestroyed();
            var count = 0;
            foreach (var item in _subscriptions) {
                count += item.Value.Count;
            }
            return count;
        }

        public int GetDecisionSubscriptionCount() {
            ThrowIfNotCreatedOrDestroyed();
>>>>>>> Stashed changes
            var count = 0;
            foreach (var kv in _decisionSubscriptions) {
                count += kv.Value.Count;
            }
            return count;
        }

<<<<<<< Updated upstream
        public int GetVoteExecutorCount() {
            var count = 0;
            foreach (var kv in _voteExecutorLists) {
                count += kv.Value.Count;
            }
            return count;
        }

        protected override void OnCreate() {
            _eventExecutorLists = new Dictionary<int, List<EventExecutor>>();
            _voteExecutorLists = new Dictionary<int, List<VoteExecutor>>();
        }

        protected override void OnDestroy() {
            _eventExecutorLists = null;
            _voteExecutorLists = null;
        }
=======
        public int GetTotalSubscriptionCount() {
            ThrowIfNotCreatedOrDestroyed();
            return GetInteractionSubscriptionCount() + GetDecisionSubscriptionCount();
        }

        public DiagnosticsSnapshot GetDiagnostics() {
            ThrowIfNotCreatedOrDestroyed();
            return new DiagnosticsSnapshot(
                GetInteractionSubscriptionCount(),
                GetDecisionSubscriptionCount());
        }

        protected override void OnCreate() {
            _subscriptions = new Dictionary<Type, List<InteractionSubscription>>();
            _decisionSubscriptions = new Dictionary<Type, List<DecisionSubscription>>();

            _subscriptionPool = new InteractionSubscriptionPool();
            _subscriptionPool.Create();

            _decisionPool = new DecisionSubscriptionPool();
            _decisionPool.Create();
        }

        protected override void OnDestroy() {
            ClearSubscriptions();
            ClearDecisionSubscriptions();

            _subscriptionPool?.Destroy();
            _decisionPool?.Destroy();

            _subscriptions = null;
            _decisionSubscriptions = null;
            _subscriptionPool = null;
            _decisionPool = null;
        }

        private InteractionSubscription SubscribeInternal<TMessage>(Action<TMessage> action, ILifetime lifetime)
            where TMessage : class, IInteractionMessage {
            ThrowHelper.ThrowIfNull(action, nameof(action));

            var subscription = _subscriptionPool.Get();
            subscription.Handle = _index++;
            subscription.MessageType = typeof(TMessage);
            subscription.Action = action;

            if (!_subscriptions.TryGetValue(typeof(TMessage), out var subscriptions)) {
                subscriptions = _subscriptions[typeof(TMessage)] = new List<InteractionSubscription>();
            }

            subscriptions.Add(subscription);
            lifetime?.Add(subscription);
            return subscription;
        }

        private DecisionSubscription ListenInternal<TDecision>(Func<TDecision, bool> handler, ILifetime lifetime)
            where TDecision : class, IDecisionMessage {
            ThrowHelper.ThrowIfNull(handler, nameof(handler));

            var subscription = _decisionPool.Get();
            subscription.Handle = _index++;
            subscription.DecisionType = typeof(TDecision);
            subscription.Handler = handler;

            if (!_decisionSubscriptions.TryGetValue(typeof(TDecision), out var subscriptions)) {
                subscriptions = _decisionSubscriptions[typeof(TDecision)] = new List<DecisionSubscription>();
            }

            subscriptions.Add(subscription);
            lifetime?.Add(subscription);
            return subscription;
        }

        private IInteractionSubscription BindSubscriptionToOwner(IInteractionSubscription subscription, BaseObject owner) {
            owner.OwnLifetime(subscription);
            return subscription;
        }

        private IDecisionSubscription BindDecisionToOwner(IDecisionSubscription subscription, BaseObject owner) {
            owner.OwnLifetime(subscription);
            return subscription;
        }

        private void CleanupDestroyedSubscriptions(List<InteractionSubscription> subscriptions) {
            for (var i = subscriptions.Count - 1; i >= 0; i--) {
                if (subscriptions[i].Destroyed) {
                    _subscriptionPool.Return(subscriptions[i]);
                    subscriptions.RemoveAt(i);
                }
            }
        }

        private void CleanupDestroyedDecisionSubscriptions(List<DecisionSubscription> subscriptions) {
            for (var i = subscriptions.Count - 1; i >= 0; i--) {
                if (subscriptions[i].Destroyed) {
                    _decisionPool.Return(subscriptions[i]);
                    subscriptions.RemoveAt(i);
                }
            }
        }

        private void ClearSubscriptions() {
            if (_subscriptions == null) {
                return;
            }

            foreach (var pair in _subscriptions) {
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

            _subscriptions.Clear();
        }

        private void ClearDecisionSubscriptions() {
            if (_decisionSubscriptions == null) {
                return;
            }

            foreach (var pair in _decisionSubscriptions) {
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
                    _decisionPool.Return(subscription);
                }

                subscriptions.Clear();
            }

            _decisionSubscriptions.Clear();
        }
>>>>>>> Stashed changes
    }
}