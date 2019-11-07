//------------------------------------------------------------
//        File:  ObjectPool.cs
//       Brief:  ObjectPool
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-07-09 19:09
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System.Collections.Generic;

namespace vFrame.Core.ObjectPools
{
    public abstract class ObjectPool<TClass> where TClass : class, new()
    {
        private const int InitSize = 128;
        private static readonly Stack<TClass> Objects;

        static ObjectPool() {
            Objects = new Stack<TClass>(InitSize);

            for (var i = 0; i < InitSize; i++)
                Objects.Push(new TClass());
        }

        public static TClass Get() {
            return Objects.Count > 0 ? Objects.Pop() : new TClass();
        }

        public static void Return(TClass obj) {
            if (Objects.Contains(obj))
                return;
            Objects.Push(obj);
        }
    }

    public abstract class ObjectPool<TClass, TAllocator>
        where TClass : class, new()
        where TAllocator : IPoolObjectAllocator<TClass>, new()
    {
        private const int InitSize = 128;
        private static readonly Stack<TClass> Objects;
        private static readonly TAllocator Allocator;

        static ObjectPool() {
            Objects = new Stack<TClass>(InitSize);
            Allocator = new TAllocator();

            for (var i = 0; i < InitSize; i++)
                Objects.Push(Allocator.Alloc());
        }

        public static TClass Get() {
            return Objects.Count > 0 ? Objects.Pop() : Allocator.Alloc();
        }

        public static void Return(TClass obj) {
            Allocator.Reset(obj);

            if (Objects.Contains(obj))
                return;
            Objects.Push(obj);
        }
    }
}