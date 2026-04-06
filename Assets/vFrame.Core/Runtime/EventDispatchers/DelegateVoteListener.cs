//------------------------------------------------------------
//       @file  DelegateVoteListener.cs
//      @brief  代理事件侦听器
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//     Created  2016-09-20 20:35
//   Copyright  Copyright (c) 2024, VyronLee
//============================================================

using System;
using vFrame.Core.ObjectPools;

namespace vFrame.Core.EventDispatchers
{
    public class DelegateVoteListener : IVoteListener, IPoolObjectResetable
    {
        /// <summary>
        ///     代理接口
        /// </summary>
        public Func<IVote, bool> VoteAction;

        /// <summary>
        ///     投票响应接口
        /// </summary>
        public bool OnVote(IVote e) {
            return null != VoteAction && VoteAction(e);
        }

        public void Reset() {
            VoteAction = null;
        }
    }
}
