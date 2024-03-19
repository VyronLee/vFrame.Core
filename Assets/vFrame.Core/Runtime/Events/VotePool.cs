//------------------------------------------------------------
//        File:  VotePool.cs
//       Brief:  Vote pool.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-08 23:16
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using vFrame.Core.ObjectPools;

namespace vFrame.Core.Events
{
    internal class VotePool : ObjectPool<Vote, VoteAllocator> { }

    internal class VoteAllocator : IPoolObjectAllocator<Vote>
    {
        public Vote Alloc() {
            return new Vote();
        }

        public void Reset(Vote obj) {
            obj.Context = null;
            obj.Target = null;
            obj.VoteId = 0;
        }
    }
}