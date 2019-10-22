//------------------------------------------------------------
//       @file  EventDispatcher.cs
//      @brief  事件派发器
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-07-31 22:34
//   Copyright  Copyright (c) 2016, VyronLee
//============================================================

using System;
using System.Collections.Generic;
using vFrame.Core.Components;
using vFrame.Core.Loggers;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.Events
{
    public class EventDispatcher : Component, IEventDispatcher
    {
        private static readonly LogTag EventLogTag = new LogTag("EventDispatcher");
        private uint _index = 1;

        private Dictionary<int, List<EventExecutor>> _eventExecutorLists;
        private Dictionary<int, List<VoteExecutor>> _voteExecutorLists;

        protected override void OnCreate()
        {
            _eventExecutorLists = new Dictionary<int, List<EventExecutor>>();
            _voteExecutorLists = new Dictionary<int, List<VoteExecutor>>();
        }

        protected override void OnDestroy()
        {
            _eventExecutorLists = null;
            _voteExecutorLists = null;
        }

        public uint AddEventListener(IEventListener listener, int eventId)
        {
            if (listener == null)
                throw new ArgumentNullException("listener");

            var executor = EventExecutorPool.Get();
            executor.EventId = eventId;
            executor.Listener = listener;
            executor.Handle = _index++;

            if (!_eventExecutorLists.ContainsKey(eventId))
                _eventExecutorLists.Add(eventId, new List<EventExecutor>());

            _eventExecutorLists[eventId].Add(executor);

            return executor.Handle;
        }

        public uint AddEventListener(Action<IEvent> action, int eventId)
        {
            var delegateEventListener = ObjectPool<DelegateEventListener>.Get();
            delegateEventListener.Action = action;

            return AddEventListener(delegateEventListener, eventId);
        }

        public IEventListener RemoveEventListener(uint handle)
        {
            IEventListener listener = null;
            foreach (var item in _eventExecutorLists)
            {
                var list = item.Value;
                foreach (var e in list)
                    if (e.Handle == handle)
                    {
                        e.Stop();
                        listener = e.Listener;
                    }
            }

            var eventListener = listener as DelegateEventListener;
            if (eventListener != null)
                ObjectPool<DelegateEventListener>.Return(eventListener);

            return null;
        }

        public void DispatchEvent(int eventId)
        {
            DispatchEvent(eventId, null);
        }

        public void DispatchEvent(int eventId, object context)
        {
            if (!_eventExecutorLists.ContainsKey(eventId))
                return;

            var executorList = _eventExecutorLists[eventId];

            // 移除已经停止的执行器
            var len = executorList.Count;
            for (var i = len - 1; i >= 0; --i)
                if (executorList[i].Stopped)
                {
                    EventExecutorPool.Return(executorList[i]);
                    executorList.RemoveAt(i);
                }

            // 必须先激活已存在的执行器, 执行过程中新增的执行器不需要生效
            foreach (var executor in executorList)
                executor.Activate();

            // 循环执行
            var e = EventPool.Get();
            e.EventId = eventId;
            e.Context = context;
            e.Target = this;

            foreach (var executor in executorList)
            {
                if (!executor.Activated || executor.Stopped)
                    continue;

                try
                {
                    executor.Execute(e);
                }
                catch (Exception exception)
                {
                    Logger.Error(EventLogTag, "Exception occur, event id: {0}, msg: {1}",
                        eventId, exception);
                }
            }
            EventPool.Return(e);
        }

        public uint AddVoteListener(IVoteListener listener, int voteId)
        {
            var executor = VoteExecutorPool.Get();
            executor.VoteId = voteId;
            executor.Listener = listener;
            executor.Handle = _index++;

            if (!_voteExecutorLists.ContainsKey(voteId))
                _voteExecutorLists.Add(voteId, new List<VoteExecutor>());

            _voteExecutorLists[voteId].Add(executor);

            return executor.Handle;
        }

        public uint AddVoteListener(Func<IVote, bool> func, int voteId)
        {
            var listener = ObjectPool<DelegateVoteListener>.Get();
            listener.VoteAction = func;

            return AddVoteListener(listener, voteId);
        }

        public IVoteListener RemoveVoteListener(uint handle)
        {
            IVoteListener listener = null;
            foreach (var item in _voteExecutorLists)
            {
                var list = item.Value;
                foreach (var v in list)
                    if (v.Handle == handle)
                    {
                        v.Stop();
                        listener = v.Listener;
                        break;
                    }
            }

            var voteListener = listener as DelegateVoteListener;
            if (voteListener != null)
                ObjectPool<DelegateVoteListener>.Return(voteListener);

            return null;
        }

        public bool DispatchVote(int voteId)
        {
            return DispatchVote(voteId, null);
        }

        public bool DispatchVote(int voteId, object context)
        {
            if (!_voteExecutorLists.ContainsKey(voteId))
                return true;

            var executorList = _voteExecutorLists[voteId];

            // 移除已经停止的执行器
            var len = executorList.Count;
            for (var i = len - 1; i >= 0; --i)
                if (executorList[i].Stopped)
                {
                    VoteExecutorPool.Return(executorList[i]);
                    executorList.RemoveAt(i);
                }

            // 必须先激活已存在的执行器, 执行过程中新增的执行器不需要生效
            foreach (var executor in executorList)
                executor.Activate();

            // 循环执行
            var e = VotePool.Get();
            e.VoteId = voteId;
            e.Context = context;
            e.Target = this;

            var pass = true;
            foreach (var executor in executorList)
            {
                if (!executor.Activated || executor.Stopped)
                    continue;

                try
                {
                    if (executor.Execute(e))
                        continue;

                    pass = false;
                    break;
                }
                catch (Exception exception)
                {
                    Logger.Error(EventLogTag, "Exception occur, vote id: {0}, msg: {1}",
                        voteId, exception);
                }
            }

            VotePool.Return(e);
            return pass;
        }

        public void RemoveAllListeners()
        {
            _eventExecutorLists.Clear();
            _voteExecutorLists.Clear();
        }

        public int GetEventExecutorCount()
        {
            var count = 0;
            foreach (var kv in _eventExecutorLists)
                count += kv.Value.Count;

            return count;
        }

        public int GetVoteExecutorCount()
        {
            var count = 0;
            foreach (var kv in _voteExecutorLists)
                count += kv.Value.Count;

            return count;
        }
    }
}