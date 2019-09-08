//------------------------------------------------------------
//       @file  DelegateVoteListener.cs
//      @brief  代理事件侦听器
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-09-20 20:35
//   Copyright  Copyright (c) 2016, VyronLee
//============================================================

using vFrame.Core.Interface.Events;
using vFrame.Core.Pool;

namespace vFrame.Core.Events
{
    public class DelegateVoteListener : Poolable, IVoteListener
    {
        /// <summary>
        /// 代理接口
        /// </summary>
        public VoteDelegate voteDelegate;

        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            voteDelegate = null;
        }

        /// <summary>
        /// 投票响应接口
        /// </summary>
        public bool OnVote(IVote e)
        {
            if (null != voteDelegate)
            {
                return voteDelegate(e);
            }

            return false;
        }

        protected override void OnCreate()
        {

        }

        protected override void OnDestroy()
        {

        }
    }
}