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
using JetBrains.Annotations;
using UnityEngine;
using vFrame.Core.Interface.Components;
using vFrame.Core.Interface.Events;
using vFrame.Core.Pool;
using Component = vFrame.Core.Components.Component;

namespace vFrame.Core.Events
{
    public class EventDispatcher : Components.Component, IEventDispatcher
    {
        private static int _index = 1;

        private Dictionary<int, List<EventExecutor>> _eventExecutorLists;
        private Dictionary<int, List<VoteExecutor>> _voteExecutorLists;

        public int AddEventListener([NotNull] IEventListener listener, int eventId)
        {
            if (listener == null)
                throw new ArgumentNullException("listener");
            return AddEventListener(listener, eventId, 0);
        }

        public int AddEventListener([NotNull] IEventListener listener, int eventId, int priority = 0)
        {
            if (listener == null)
                throw new ArgumentNullException("listener");

            var executor = ObjectPools<EventExecutor>.Spawn();
            executor.eventId = eventId;
            executor.priority = priority;
            executor.listener = listener;
            executor.handle = _index++;

            if (!_eventExecutorLists.ContainsKey(eventId))
                _eventExecutorLists.Add(eventId, new List<EventExecutor>());

            _eventExecutorLists[eventId].Add(executor);

            return executor.handle;
        }

        public int AddEventListener(EventDelegate eventDelegate, int eventId)
        {
            return AddEventListener(eventDelegate, eventId, 0);
        }

        public int AddEventListener(EventDelegate eventDelegate, int eventId, int priority = 0)
        {
            var delegateEventListener = ObjectPools<DelegateEventListener>.Spawn();
            delegateEventListener.eventDelegate = eventDelegate;

            return AddEventListener(delegateEventListener, eventId, priority);
        }

        public IEventListener RemoveEventListener(int handle)
        {
            IEventListener listener = null;
            foreach (var item in _eventExecutorLists)
            {
                var list = item.Value;
                foreach (var e in list)
                    if (e.handle == handle)
                    {
                        e.Stop();
                        listener = e.listener;
                    }
            }

            var eventListener = listener as DelegateEventListener;
            if (eventListener != null)
                ObjectPools<DelegateEventListener>.Recycle(eventListener);

            return null;
        }

        public void DispatchEvent(int eventId)
        {
            DispatchEvent(eventId, null);
        }

        public void DispatchEvent(int eventId, object context)
        {
            //Debug.LogFormat("EventDispatcher:DispatchEvent - eventId: {0}", eventId);
            if (!_eventExecutorLists.ContainsKey(eventId)) return;

            var executorList = _eventExecutorLists[eventId];

            // 移除已经停止的执行器
            var len = executorList.Count;
            for (var i = len - 1; i >= 0; --i)
                if (executorList[i].IsStopped())
                {
                    ObjectPools<EventExecutor>.Recycle(executorList[i]);
                    executorList.RemoveAt(i);
                }

            // 根据优先级对执行器列表进行排序
            executorList.Sort(delegate(EventExecutor x, EventExecutor y)
            {
                if (x.priority < y.priority)
                    return 1;
                if (x.priority > y.priority)
                    return -1;
                if (x.handle > y.handle)
                    return 1;
                if (x.handle < y.handle)
                    return -1;
                return 0;
            });

            // 必须先激活已存在的执行器, 执行过程中新增的执行器不需要生效
            executorList.ForEach(obj => obj.Activate());

            // 循环执行
            var e = ObjectPools<Event>.Spawn();
            e.eventId = eventId;
            e.context = context;
            e.target = this;

            executorList.ForEach(delegate(EventExecutor obj)
            {
                if (!obj.IsActivated() || obj.IsStopped()) return;

                try
                {
                    obj.Execute(e);
                }
                catch (Exception exception)
                {
                    Debug.LogErrorFormat("EventDispatcher:DispatchEvent - exception occur, event id: {0}, msg: {1}",
                        eventId, exception);
                }
            });
            ObjectPools<Event>.Recycle(e);
        }

        public int AddVoteListener(IVoteListener listener, int voteId, int priority = 0)
        {
            var executor = ObjectPools<VoteExecutor>.Spawn();
            executor.voteId = voteId;
            executor.priority = priority;
            executor.listener = listener;
            executor.handle = _index++;

            if (!_voteExecutorLists.ContainsKey(voteId))
                _voteExecutorLists.Add(voteId, new List<VoteExecutor>());

            _voteExecutorLists[voteId].Add(executor);

            return executor.handle;
        }

        public int AddVoteListener(VoteDelegate voteDelegate, int voteId, int priority = 0)
        {
            var delegateVoteListener = ObjectPools<DelegateVoteListener>.Spawn();
            delegateVoteListener.voteDelegate = voteDelegate;

            return AddVoteListener(delegateVoteListener, voteId, priority);
        }

        public IVoteListener RemoveVoteListener(int handle)
        {
            IVoteListener listener = null;
            foreach (var item in _voteExecutorLists)
            {
                var list = item.Value;
                foreach (var v in list)
                    if (v.handle == handle)
                    {
                        v.Stop();
                        listener = v.listener;
                        break;
                    }
            }

            var voteListener = listener as DelegateVoteListener;
            if (voteListener != null)
                ObjectPools<DelegateVoteListener>.Recycle(voteListener);

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
                if (executorList[i].IsStopped())
                {
                    ObjectPools<VoteExecutor>.Recycle(executorList[i]);
                    executorList.RemoveAt(i);
                }

            // 根据优先级对执行器列表进行排序
            executorList.Sort(delegate(VoteExecutor x, VoteExecutor y)
            {
                if (x.priority > y.priority) return 1;

                if (x.priority < y.priority) return -1;

                return 0;
            });

            // 必须先激活已存在的执行器, 执行过程中新增的执行器不需要生效
            executorList.ForEach(obj => obj.Activate());

            // 循环执行
            var e = ObjectPools<Vote>.Spawn();
            e.voteId = voteId;
            e.context = context;
            e.target = this;

            var pass = true;
            foreach (var executor in executorList)
            {
                if (!executor.IsActivated() || executor.IsStopped()) continue;

                try
                {
                    if (executor.Execute(e))
                        continue;

                    pass = false;
                    break;
                }
                catch (Exception exception)
                {
                    Debug.LogErrorFormat("EventDispatcher:DispatchVote - Exception occur, vote id: {0}, msg: {1}",
                        voteId, exception);
                }
            }

            ObjectPools<Vote>.Recycle(e);
            return pass;
        }

        public void RemoveAllListeners()
        {
            _eventExecutorLists = new Dictionary<int, List<EventExecutor>>();
            _voteExecutorLists = new Dictionary<int, List<VoteExecutor>>();
        }

        public int GetEventExecutorCount()
        {
            var count = 0;
            foreach (var kv in _eventExecutorLists) count += kv.Value.Count;

            return count;
        }

        public int GetVoteExecutorCount()
        {
            var count = 0;
            foreach (var kv in _voteExecutorLists) count += kv.Value.Count;

            return count;
        }

        protected override void OnCreate()
        {
            _eventExecutorLists = new Dictionary<int, List<EventExecutor>>();
            _voteExecutorLists = new Dictionary<int, List<VoteExecutor>>();
        }

        protected override void OnDestroy()
        {
            _eventExecutorLists.Clear();
            _voteExecutorLists.Clear();
        }

        protected override void OnBind(IBindable target)
        {
        }

        protected override void OnUnbind(IBindable target)
        {
        }
    }
}