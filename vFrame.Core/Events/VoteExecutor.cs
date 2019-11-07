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

using vFrame.Core.Base;

namespace vFrame.Core.Events
{
    public class VoteExecutor : BaseObject
    {
        public bool Activated { get; set; }
        public bool Stopped { get; set; }

        public uint Handle;
        public IVoteListener Listener;
        public int VoteId;

        /// <summary>
        ///     创建函数
        /// </summary>
        protected override void OnCreate() {
        }

        /// <summary>
        ///     创建函数
        /// </summary>
        protected override void OnDestroy() {
            Listener = null;
        }

        /// <summary>
        ///     激活
        /// </summary>
        public void Activate() {
            Activated = true;
        }

        /// <summary>
        ///     停止
        /// </summary>
        public void Stop() {
            Stopped = true;
        }

        /// <summary>
        ///     执行
        /// </summary>
        public bool Execute(IVote e) {
            if (null != Listener)
                return Listener.OnVote(e);
            return false;
        }
    }
}