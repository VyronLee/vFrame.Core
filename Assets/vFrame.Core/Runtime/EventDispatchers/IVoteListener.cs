﻿//------------------------------------------------------------
//       @file  IVoteListener.cs
//      @brief  投票侦听器
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//     Created  2016-07-31 22:31
//   Copyright  Copyright (c) 2024, VyronLee
//============================================================

namespace vFrame.Core.EventDispatchers
{
    public interface IVoteListener
    {
        /// <summary>
        ///     投票响应接口
        /// </summary>
        bool OnVote(IVote e);
    }
}