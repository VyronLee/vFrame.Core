//------------------------------------------------------------
//       @file  Poolable.cs
//      @brief  可缓存对象
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-07-30 17:07
//   Copyright  Copyright (c) 2016, VyronLee
//============================================================

using vFrame.Core.Base;

namespace vFrame.Core.Pool
{
    public abstract class Poolable : BaseObject
    {
        /// <summary>
        /// 重置函数
        /// </summary>
        public abstract void Reset();
    }
}