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

namespace vFrame.Core.EventDispatchers
{
    /// <summary>
    /// Full retained dispatcher surface. Typed interaction is the default path for new work,
    /// `int eventId` APIs are compatibility-oriented migration paths, and `Vote` / `Decision`
    /// remain explicit specialized semantics.
    /// </summary>
    public interface IEventDispatcher : IInteractionDispatcher
    {
        /// <summary>
        ///     添加事件侦听器
        ///     - 兼容保留的 `int eventId` 事件路径，供迁移场景继续使用
        ///     - 如果在事件派发过程中添加事件，该侦听器会在下一个事件派发才生效
        /// </summary>
        /// <param name="listener">侦听器</param>
        /// <param name="eventId">事件ID</param>
        uint AddEventListener(IEventListener listener, int eventId);

        /// <summary>
        ///     添加事件代理侦听器
        ///     - 兼容保留的 `int eventId` 事件路径，供迁移场景继续使用
        ///     - 如果在事件派发过程中添加事件，该侦听器会在下一个事件派发才生效
        /// </summary>
        /// <param name="listener">侦听器</param>
        /// <param name="eventId">事件ID</param>
        uint AddEventListener(Action<IEvent> listener, int eventId);

        /// <summary>
        ///     移除事件侦听器
        ///     - 用于兼容保留的 `int eventId` 事件路径
        ///     - 如果处于事件派发过程中，该侦听器不会马上从队列移除，以防止迭代出错，
        ///     但该侦听器会立即失效。
        /// </summary>
        /// <param name="handle">事件句柄</param>
        IEventListener RemoveEventListener(uint handle);

        /// <summary>
        ///     派发事件
        ///     - 兼容保留的 `int eventId` 事件路径，不是新交互的首选入口
        /// </summary>
        /// <param name="eventId">事件ID</param>
        void DispatchEvent(int eventId);

        /// <summary>
        ///     派发事件
        ///     - 兼容保留的 `int eventId` 事件路径，不是新交互的首选入口
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <param name="context">事件现场</param>
        void DispatchEvent(int eventId, object context);

        /// <summary>
        ///     添加投票侦听器
        ///     - 投票是显式保留的规则/裁决语义，不属于普通消息派发
        /// </summary>
        uint AddVoteListener(IVoteListener listener, int voteId);

        /// <summary>
        ///     添加决策侦听器
        ///     - 决策是投票语义的专用业务语义入口，不属于普通事件派发
        /// </summary>
        uint AddDecisionListener(IVoteListener listener, int decisionId);

        /// <summary>
        ///     添加代理投票侦听器
        ///     - 投票是显式保留的规则/裁决语义，不属于普通消息派发
        /// </summary>
        uint AddVoteListener(Func<IVote, bool> voteDelegate, int voteId);

        /// <summary>
        ///     添加代理决策侦听器
        ///     - 决策是投票语义的专用业务语义入口，不属于普通事件派发
        /// </summary>
        uint AddDecisionListener(Func<IVote, bool> decisionDelegate, int decisionId);

        /// <summary>
        ///     移除投票侦听器
        ///     - 用于显式投票/决策语义，不属于普通事件移除路径
        /// </summary>
        IVoteListener RemoveVoteListener(uint handle);

        /// <summary>
        ///     移除决策侦听器
        /// </summary>
        IVoteListener RemoveDecisionListener(uint handle);

        /// <summary>
        ///     派发投票
        ///     - 投票是显式保留的规则/裁决语义，不属于普通消息派发
        /// </summary>
        /// <param name="voteId">投票ID</param>
        /// <param name="context">投票现场</param>
        /// <returns>投票是否通过</returns>
        bool DispatchVote(int voteId, object context);

        /// <summary>
        ///     派发决策
        ///     - 决策是投票语义的专用业务语义入口，不属于普通事件派发
        /// </summary>
        /// <param name="decisionId">决策ID</param>
        /// <param name="context">决策现场</param>
        /// <returns>决策是否通过</returns>
        bool DispatchDecision(int decisionId, object context);

        /// <summary>
        ///     派发投票
        ///     - 投票是显式保留的规则/裁决语义，不属于普通消息派发
        /// </summary>
        /// <param name="voteId">投票ID</param>
        /// <returns>投票是否通过</returns>
        bool DispatchVote(int voteId);

        /// <summary>
        ///     派发决策
        /// </summary>
        /// <param name="decisionId">决策ID</param>
        /// <returns>决策是否通过</returns>
        bool DispatchDecision(int decisionId);

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
