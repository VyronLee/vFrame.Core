//------------------------------------------------------------
//        File:  VoteExecutorPool.cs
//       Brief:  Vote executor pool.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-08 23:21
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using vFrame.Core.ObjectPools;

namespace vFrame.Core.EventDispatchers
{
    internal class VoteExecutorPool : ObjectPool<VoteExecutor, VoteExecutorAllocator> { }

    internal class VoteExecutorAllocator : IPoolObjectAllocator<VoteExecutor>
    {
        public VoteExecutor Alloc() {
            return new VoteExecutor();
        }

        public void Reset(VoteExecutor obj) {
            obj.Activated = false;
            obj.Stopped = false;
            obj.Handle = 0;
            obj.Listener = null;
            obj.VoteId = 0;
        }
    }
}