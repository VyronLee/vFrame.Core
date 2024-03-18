//------------------------------------------------------------
//       @file  IVote.cs
//      @brief  投票接口
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-07-31 22:24
//   Copyright  Copyright (c) 2024, VyronLee
//============================================================

namespace vFrame.Core.Events
{
    public interface IVote
    {
        /// <summary>
        /// 获取投票ID
        /// </summary>
        int GetVoteID();

        /// <summary>
        /// 获取投票现场
        /// </summary>
        object GetContext();

        /// <summary>
        /// 获取投票发送者
        /// </summary>
        object GetVoteTarget();
    }
}