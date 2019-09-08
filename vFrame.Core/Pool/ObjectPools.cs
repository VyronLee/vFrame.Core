//------------------------------------------------------------
//       @file  ObjectPools.cs
//      @brief  对象池
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-07-30 17:05
//   Copyright  Copyright (c) 2016, VyronLee
//============================================================

using System.Collections.Generic;

namespace vFrame.Core.Pool
{
    public static class ObjectPools<T> where T : Poolable, new()
    {
        /// <summary>
        /// 对象列表
        /// </summary>
        private static readonly Stack<T> Objects = new Stack<T>();

        /// <summary>
        /// 最大容量, 默认100
        /// </summary>
        public static int MaxCount = 100;

        /// <summary>
        /// 获取对象
        ///   - 如果池中有对象，则返回一个；
        ///   - 如果没有，则新生成一个；
        /// </summary>
        public static T Spawn()
        {
            if (Objects.Count > 0)
                return Objects.Pop();

            return new T();
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        public static void Recycle(T obj)
        {
            obj.Reset();

            if (Objects.Count < MaxCount && !Objects.Contains(obj))
            {
                Objects.Push(obj);
            }
        }
    }
}