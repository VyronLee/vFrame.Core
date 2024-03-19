//------------------------------------------------------------
//       @file  Vote.cs
//      @brief  投票
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//     Created  2016-08-03 18:03
//   Copyright  Copyright (c) 2024, VyronLee
//============================================================

using vFrame.Core.Base;

namespace vFrame.Core.Events
{
    public class Vote : BaseObject, IVote
    {
        public object Context;
        public EventDispatcher Target;
        public int VoteId;

        /// <summary>
        ///     获取投票ID
        /// </summary>
        public int GetVoteID() {
            return VoteId;
        }

        /// <summary>
        ///     获取事件现场
        /// </summary>
        public object GetContext() {
            return Context;
        }

        /// <summary>
        ///     获取投票发送者
        /// </summary>
        public object GetVoteTarget() {
            return Target;
        }

        /// <summary>
        ///     创建函数
        /// </summary>
        protected override void OnCreate() { }

        /// <summary>
        ///     销毁函数
        /// </summary>
        protected override void OnDestroy() {
            Target = null;
        }
    }
}