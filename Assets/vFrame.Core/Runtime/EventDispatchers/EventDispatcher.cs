//------------------------------------------------------------
//       @file  EventDispatcher.cs
//      @brief  事件派发器
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//     Created  2016-07-31 22:34
//   Copyright  Copyright (c) 2024, VyronLee
//============================================================

using System;
using System.Collections.Generic;
using vFrame.Core.Base;
using vFrame.Core.Exceptions;
using vFrame.Core.Loggers;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.EventDispatchers
{
    public class EventDispatcher : BaseObject, IEventDispatcher
    {
        public readonly struct DiagnosticsSnapshot
        {
            public DiagnosticsSnapshot(int eventExecutorCount, int interactionSubscriptionCount, int voteExecutorCount) {
                EventExecutorCount = eventExecutorCount;
                InteractionSubscriptionCount = interactionSubscriptionCount;
                VoteExecutorCount = voteExecutorCount;
            }

            public int EventExecutorCount { get; }

            public int InteractionSubscriptionCount { get; }

            public int VoteExecutorCount { get; }
        }

        private static readonly LogTag EventLogTag = new LogTag("EventDispatcher");

        private Dictionary<int, List<EventExecutor>> _eventExecutorLists;
        private Dictionary<Type, List<InteractionSubscription>> _interactionSubscriptions;
        private uint _index = 1;
        private Dictionary<int, List<VoteExecutor>> _voteExecutorLists;

        /// <summary>
        /// Retained interaction entry point. Typed publish-subscribe is the default path for new
        /// core interaction work, while `int eventId` dispatch remains as a compatibility layer.
        /// </summary>
        /// <summary>
        /// Creates a bare subscription. The caller borrows dispatcher access and remains
        /// responsible for explicit unsubscription.
        /// </summary>
        public IInteractionSubscription Subscribe<TMessage>(Action<TMessage> action) where TMessage : class {
            return SubscribeInternal(action, null);
        }

        /// <summary>
        /// Creates an owner-bound subscription. The dispatcher borrows the owner lifecycle and
        /// ends the subscription automatically when the owner is destroyed.
        /// </summary>
        public IInteractionSubscription Subscribe<TMessage>(Action<TMessage> action, BaseObject owner) where TMessage : class {
            ThrowHelper.ThrowIfNull(owner, nameof(owner));

            var subscription = SubscribeInternal(action, null);
            return BindSubscriptionToOwner(subscription, owner);
        }

        /// <summary>
        /// Creates a lifetime-bound subscription. The dispatcher borrows the provided lifetime and
        /// ends the subscription when that lifetime terminates.
        /// </summary>
        public IInteractionSubscription Subscribe<TMessage>(Action<TMessage> action, ILifetime lifetime) where TMessage : class {
            ThrowHelper.ThrowIfNull(lifetime, nameof(lifetime));

            var subscription = SubscribeInternal(action, lifetime);
            return subscription;
        }

        public void Unsubscribe(IInteractionSubscription subscription) {
            if (subscription == null || subscription.Destroyed) {
                return;
            }

            subscription.Destroy();
        }

        /// <summary>
        /// Publishes a typed message through the retained default interaction path.
        /// </summary>
        public void Publish<TMessage>(TMessage message) where TMessage : class {
            ThrowIfNotCreatedOrDestroyed();
            ThrowHelper.ThrowIfNull(message, nameof(message));

            if (!_interactionSubscriptions.TryGetValue(typeof(TMessage), out var subscriptions)) {
                return;
            }

            CleanupStoppedInteractionSubscriptions(subscriptions);

            for (var i = 0; i < subscriptions.Count; i++) {
                var subscription = subscriptions[i];
                if (subscription.Destroyed) {
                    continue;
                }

                try {
                    ((Action<TMessage>)subscription.Action)?.Invoke(message);
                }
                catch (Exception exception) {
                    Logger.Error(EventLogTag, "Exception occurred, interaction type: {0}, exception: {1}",
                        typeof(TMessage).FullName, exception);
                }
            }
        }

        public int GetInteractionSubscriptionCount() {
            ThrowIfNotCreatedOrDestroyed();
            var count = 0;
            foreach (var item in _interactionSubscriptions) {
                count += item.Value.Count;
            }
            return count;
        }

        /// <summary>
        /// Adds a legacy numeric-event listener. This retained `int eventId` path remains available
        /// for migration compatibility, while typed messages are the default path for new work.
        /// </summary>
        public uint AddEventListener(IEventListener listener, int eventId) {
            ThrowIfNotCreatedOrDestroyed();
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

        /// <summary>
        /// Adds a delegate listener for the retained compatibility-only numeric event path.
        /// </summary>
        public uint AddEventListener(Action<IEvent> action, int eventId) {
            ThrowIfNotCreatedOrDestroyed();
            ThrowHelper.ThrowIfNull(action, nameof(action));

            var delegateEventListener = ObjectPool<DelegateEventListener>.Shared.Get();
            delegateEventListener.Action = action;

            return AddEventListener(delegateEventListener, eventId);
        }

        /// <summary>
        /// Removes a listener from the retained compatibility-only numeric event path.
        /// </summary>
        public IEventListener RemoveEventListener(uint handle) {
            ThrowIfNotCreatedOrDestroyed();
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

        /// <summary>
        /// Dispatches a retained compatibility-only numeric event.
        /// </summary>
        public void DispatchEvent(int eventId) {
            DispatchEvent(eventId, null);
        }

        /// <summary>
        /// Dispatches a retained compatibility-only numeric event with context.
        /// </summary>
        public void DispatchEvent(int eventId, object context) {
            ThrowIfNotCreatedOrDestroyed();
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
                    continue;
                }

                try {
                    executor.Execute(e);
                }
                catch (Exception exception) {
                    Logger.Error(EventLogTag, "Exception occurred, event id: {0}, exception: {1}",
                        eventId, exception);
                }
            }

            EventPool.Shared.Return(e);
        }

        /// <summary>
        /// Adds a listener to the explicit vote/decision semantic path rather than ordinary dispatch.
        /// </summary>
        public uint AddVoteListener(IVoteListener listener, int voteId) {
            ThrowIfNotCreatedOrDestroyed();
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

        /// <summary>
        /// Adds a listener to the explicit decision alias of vote semantics.
        /// </summary>
        public uint AddDecisionListener(IVoteListener listener, int decisionId) {
            return AddVoteListener(listener, decisionId);
        }

        /// <summary>
        /// Adds a delegate listener to the explicit vote/decision semantic path.
        /// </summary>
        public uint AddVoteListener(Func<IVote, bool> func, int voteId) {
            ThrowIfNotCreatedOrDestroyed();
            ThrowHelper.ThrowIfNull(func, nameof(func));

            var listener = ObjectPool<DelegateVoteListener>.Shared.Get();
            listener.VoteAction = func;

            return AddVoteListener(listener, voteId);
        }

        /// <summary>
        /// Adds a delegate listener to the explicit decision alias of vote semantics.
        /// </summary>
        public uint AddDecisionListener(Func<IVote, bool> decisionDelegate, int decisionId) {
            return AddVoteListener(decisionDelegate, decisionId);
        }

        /// <summary>
        /// Removes a listener from the explicit vote/decision semantic path.
        /// </summary>
        public IVoteListener RemoveVoteListener(uint handle) {
            ThrowIfNotCreatedOrDestroyed();
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

        /// <summary>
        /// Removes a listener through the explicit decision alias of vote semantics.
        /// </summary>
        public IVoteListener RemoveDecisionListener(uint handle) {
            return RemoveVoteListener(handle);
        }

        /// <summary>
        /// Dispatches the explicit vote semantic path.
        /// </summary>
        public bool DispatchVote(int voteId) {
            return DispatchVote(voteId, null);
        }

        /// <summary>
        /// Dispatches the explicit decision alias of vote semantics.
        /// </summary>
        public bool DispatchDecision(int decisionId) {
            return DispatchDecision(decisionId, null);
        }

        /// <summary>
        /// Dispatches the explicit vote semantic path with context.
        /// </summary>
        public bool DispatchVote(int voteId, object context) {
            ThrowIfNotCreatedOrDestroyed();
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
                    continue;
                }

                try {
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

        /// <summary>
        /// Dispatches the explicit decision alias of vote semantics with context.
        /// </summary>
        public bool DispatchDecision(int decisionId, object context) {
            return DispatchVote(decisionId, context);
        }

        public void RemoveAllListeners() {
            ThrowIfNotCreatedOrDestroyed();

            ClearEventExecutors();
            ClearInteractionSubscriptions();
            ClearVoteExecutors();
        }

        public int GetEventExecutorCount() {
            ThrowIfNotCreatedOrDestroyed();
            var count = 0;
            foreach (var kv in _eventExecutorLists) {
                count += kv.Value.Count;
            }
            return count;
        }

        public int GetVoteExecutorCount() {
            ThrowIfNotCreatedOrDestroyed();
            var count = 0;
            foreach (var kv in _voteExecutorLists) {
                count += kv.Value.Count;
            }
            return count;
        }

        public DiagnosticsSnapshot GetDiagnostics() {
            ThrowIfNotCreatedOrDestroyed();
            return new DiagnosticsSnapshot(GetEventExecutorCount(), GetInteractionSubscriptionCount(), GetVoteExecutorCount());
        }

        protected override void OnCreate() {
            _eventExecutorLists = new Dictionary<int, List<EventExecutor>>();
            _interactionSubscriptions = new Dictionary<Type, List<InteractionSubscription>>();
            _voteExecutorLists = new Dictionary<int, List<VoteExecutor>>();
        }

        protected override void OnDestroy() {
            ClearEventExecutors();
            ClearInteractionSubscriptions();
            ClearVoteExecutors();

            _eventExecutorLists = null;
            _interactionSubscriptions = null;
            _voteExecutorLists = null;
        }

        private InteractionSubscription BindSubscriptionToOwner(IInteractionSubscription subscription, BaseObject owner) {
            owner.OwnLifetime(subscription);
            return (InteractionSubscription)subscription;
        }

        private void CleanupStoppedInteractionSubscriptions(List<InteractionSubscription> subscriptions) {
            for (var i = subscriptions.Count - 1; i >= 0; i--) {
                if (subscriptions[i].Destroyed) {
                    subscriptions.RemoveAt(i);
                }
            }
        }

        private void ClearEventExecutors() {
            if (_eventExecutorLists == null) {
                return;
            }

            foreach (var pair in _eventExecutorLists) {
                var executors = pair.Value;
                if (executors == null) {
                    continue;
                }

                for (var i = executors.Count - 1; i >= 0; i--) {
                    if (executors[i] == null) {
                        continue;
                    }

                    EventExecutorPool.Shared.Return(executors[i]);
                }

                executors.Clear();
            }

            _eventExecutorLists.Clear();
        }

        private void ClearInteractionSubscriptions() {
            if (_interactionSubscriptions == null) {
                return;
            }

            foreach (var pair in _interactionSubscriptions) {
                var subscriptions = pair.Value;
                if (subscriptions == null) {
                    continue;
                }

                for (var i = subscriptions.Count - 1; i >= 0; i--) {
                    subscriptions[i]?.Destroy();
                }

                subscriptions.Clear();
            }

            _interactionSubscriptions.Clear();
        }

        private void ClearVoteExecutors() {
            if (_voteExecutorLists == null) {
                return;
            }

            foreach (var pair in _voteExecutorLists) {
                var executors = pair.Value;
                if (executors == null) {
                    continue;
                }

                for (var i = executors.Count - 1; i >= 0; i--) {
                    if (executors[i] == null) {
                        continue;
                    }

                    VoteExecutorPool.Shared.Return(executors[i]);
                }

                executors.Clear();
            }

            _voteExecutorLists.Clear();
        }

        private InteractionSubscription SubscribeInternal<TMessage>(Action<TMessage> action, ILifetime lifetime) where TMessage : class {
            ThrowHelper.ThrowIfNull(action, nameof(action));

            var subscription = new InteractionSubscription();
            subscription.Create();
            subscription.Handle = _index++;
            subscription.MessageType = typeof(TMessage);
            subscription.Action = action;
            subscription.SetUnsubscribe(UnsubscribeInternal);

            if (!_interactionSubscriptions.TryGetValue(typeof(TMessage), out var subscriptions)) {
                subscriptions = _interactionSubscriptions[typeof(TMessage)] = new List<InteractionSubscription>();
            }

            subscriptions.Add(subscription);
            lifetime?.Add(subscription);
            return subscription;
        }

        private void UnsubscribeInternal(InteractionSubscription subscription) {
            if (subscription == null || subscription.MessageType == null) {
                return;
            }

            if (!_interactionSubscriptions.TryGetValue(subscription.MessageType, out var subscriptions)) {
                return;
            }

            subscriptions.Remove(subscription);
        }
    }
}
