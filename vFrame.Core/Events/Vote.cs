//------------------------------------------------------------
//       @file  Vote.cs
//      @brief  投票
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-08-03 18:03
//   Copyright  Copyright (c) 2016, VyronLee
//============================================================

using vFrame.Core.Interface.Events;
using vFrame.Core.Pool;

namespace vFrame.Core.Events
{
    public class Vote : Poolable, IVote
    {
        public int voteId;
        public EventDispatcher target;
        public object context;

        /// <summary>
        /// 创建函数
        /// </summary>
        protected override void OnCreate()
        {
            Reset();
        }

        /// <summary>
        /// 销毁函数
        /// </summary>
        protected override void OnDestroy()
        {
            Reset();
        }

        /// <summary>
        ///     获取投票ID
        /// </summary>
        public int GetVoteID()
        {
            return voteId;
        }

        /// <summary>
        ///     获取事件现场
        /// </summary>
        public object GetContext()
        {
            return context;
        }

        /// <summary>
        ///     获取投票发送者
        /// </summary>
        public object GetTarget()
        {
            return target;
        }

        /// <summary>
        ///     重置
        /// </summary>
        public override void Reset()
        {
            context = null;
            voteId = 0;
            target = null;
        }
    }
}