//------------------------------------------------------------
//       @file  BaseObject.cs
//      @brief  扩展基础类型
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-07-29 11:01
//   Copyright  Copyright (c) 2016, VyronLee
//============================================================

namespace vFrame.Core.Base
{
    public abstract class BaseObject : IBaseObject
    {
        /// <summary>
        ///     创建函数
        /// </summary>
        public void Create() {
            OnCreate();
        }

        /// <summary>
        ///     释放函数
        /// </summary>
        public void Destroy() {
            OnDestroy();
        }

        /// <summary>
        ///     构造处理
        /// </summary>
        protected abstract void OnCreate();

        /// <summary>
        ///     析构处理
        /// </summary>
        protected abstract void OnDestroy();
    }

    public abstract class BaseObject<T1> : BaseObject, IBaseObject<T1>
    {
        public void Create(T1 arg1) {
            OnCreate(arg1);
        }

        protected abstract void OnCreate(T1 arg1);

        protected override void OnCreate() {
        }
    }

    public abstract class BaseObject<T1, T2> : BaseObject, IBaseObject<T1, T2>
    {
        public void Create(T1 arg1, T2 arg2) {
            OnCreate(arg1, arg2);
        }

        protected abstract void OnCreate(T1 arg1, T2 arg2);

        protected override void OnCreate() {
        }
    }

    public abstract class BaseObject<T1, T2, T3> : BaseObject, IBaseObject<T1, T2, T3>
    {
        public void Create(T1 arg1, T2 arg2, T3 arg3) {
            OnCreate(arg1, arg2, arg3);
        }

        protected abstract void OnCreate(T1 arg1, T2 arg2, T3 arg3);

        protected override void OnCreate() {
        }
    }
}