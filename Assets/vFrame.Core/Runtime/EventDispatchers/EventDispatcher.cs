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
using vFrame.Core.Containers;
using vFrame.Core.Exceptions;
using vFrame.Core.Loggers;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.EventDispatchers
{
    public class EventDispatcher : Component, IEventDispatcher
    {
        private static readonly LogTag EventLogTag = new LogTag("EventDispatcher");

        private Dictionary<int, List<EventExecutor>> _eventExecutorLists;
        private uint _index = 1;
        private Dictionary<int, List<VoteExecutor>> _voteExecutorLists;

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

        public void RemoveAllListeners() {
            _eventExecutorLists.Clear();
            _voteExecutorLists.Clear();
        }

        public int GetEventExecutorCount() {
            var count = 0;
            foreach (var kv in _eventExecutorLists) {
                count += kv.Value.Count;
            }
            return count;
        }

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
    }
}