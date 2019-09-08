//------------------------------------------------------------
//       @file  VoteExecutor.cs
//      @brief  投票执行者
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-08-01 15:56
//   Copyright  Copyright (c) 2016, VyronLee
//============================================================

using vFrame.Core.Interface.Events;
using vFrame.Core.Pool;

namespace vFrame.Core.Events
{
    public class VoteExecutor : Poolable
    {
        public bool activated;
        public int handle;
        public IVoteListener listener;
        public int priority;
        public bool stopped;
        public int voteId;

        /// <summary>
        ///     创建函数
        /// </summary>
        protected override void OnCreate()
        {
            Reset();
        }

        /// <summary>
        ///     创建函数
        /// </summary>
        protected override void OnDestroy()
        {
            Reset();
        }

        /// <summary>
        ///     重置
        /// </summary>
        public override void Reset()
        {
            handle = 0;
            voteId = 0;
            priority = 0;
            listener = null;
            activated = false;
            stopped = false;
        }

        /// <summary>
        ///     激活
        /// </summary>
        public void Activate()
        {
            activated = true;
        }

        /// <summary>
        ///     是否已经激活
        /// </summary>
        public bool IsActivated()
        {
            return activated;
        }

        /// <summary>
        ///     停止
        /// </summary>
        public void Stop()
        {
            stopped = true;
        }

        /// <summary>
        ///     是否已经停止
        /// </summary>
        public bool IsStopped()
        {
            return stopped;
        }

        /// <summary>
        ///     执行
        /// </summary>
        public bool Execute(IVote e)
        {
            if (null != listener)
                return listener.OnVote(e);
            return false;
        }
    }
}