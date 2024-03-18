//------------------------------------------------------------
//        File:  IEventDispatcher.cs
//       Brief:  事件派发器
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2018-12-14 21:58
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System;

namespace vFrame.Core.Events
{
    public interface IEventDispatcher
    {
        /// <summary>
        ///     添加事件侦听器
        ///     - 如果在事件派发过程中添加事件，该侦听器会在下一个事件派发才生效
        /// </summary>
        /// <param name="listener">侦听器</param>
        /// <param name="eventId">事件ID</param>
        uint AddEventListener(IEventListener listener, int eventId);

        /// <summary>
        ///     添加事件代理侦听器
        ///     - 如果在事件派发过程中添加事件，该侦听器会在下一个事件派发才生效
        /// </summary>
        /// <param name="listener">侦听器</param>
        /// <param name="eventId">事件ID</param>
        uint AddEventListener(Action<IEvent> listener, int eventId);

        /// <summary>
        ///     移除事件侦听器
        ///     - 如果处于事件派发过程中，该侦听器不会马上从队列移除，以防止迭代出错，
        ///     但该侦听器会立即失效。
        /// </summary>
        /// <param name="handle">事件句柄</param>
        IEventListener RemoveEventListener(uint handle);

        /// <summary>
        ///     派发事件
        /// </summary>
        /// <param name="eventId">事件ID</param>
        void DispatchEvent(int eventId);

        /// <summary>
        ///     派发事件
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <param name="context">事件现场</param>
        void DispatchEvent(int eventId, object context);

        /// <summary>
        ///     添加投票侦听器
        /// </summary>
        uint AddVoteListener(IVoteListener listener, int voteId);

        /// <summary>
        ///     添加代理投票侦听器
        /// </summary>
        uint AddVoteListener(Func<IVote, bool> voteDelegate, int voteId);

        /// <summary>
        ///     移除投票侦听器
        /// </summary>
        IVoteListener RemoveVoteListener(uint handle);

        /// <summary>
        ///     派发投票
        /// </summary>
        /// <param name="voteId">投票ID</param>
        /// <param name="context">投票现场</param>
        /// <returns>投票是否通过</returns>
        bool DispatchVote(int voteId, object context);

        /// <summary>
        ///     派发投票
        /// </summary>
        /// <param name="voteId">投票ID</param>
        /// <returns>投票是否通过</returns>
        bool DispatchVote(int voteId);

        /// <summary>
        ///     移除所有侦听器
        /// </summary>
        void RemoveAllListeners();

        /// <summary>
        ///     获取事件侦听器总个数
        /// </summary>
        /// <returns>个数</returns>
        int GetEventExecutorCount();

        /// <summary>
        ///     获取投票侦听器总个数
        /// </summary>
        /// <returns>个数</returns>
        int GetVoteExecutorCount();
    }
}