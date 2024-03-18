//------------------------------------------------------------
//       @file  BaseObject.cs
//      @brief  Base object class.
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//    Modified  2016-07-29 11:01
//   Copyright  Copyright (c) 2024, VyronLee
//============================================================

namespace vFrame.Core.Base
{
    public abstract class Object : IDestroyable
    {
        public bool Created { get; protected set; }
        public bool Destroyed { get; protected set; }

        public void Destroy() {
            if (Destroyed) {
                return;
            }
            try {
                OnDestroy();
            }
            finally {
                Destroyed = true;
                Created = false;
            }
        }

        protected abstract void OnDestroy();

        public void Dispose() {
            Destroy();
        }

        protected void ThrowIfDestroyed() {
            if (Destroyed) {
                throw new BaseObjectDestroyedException();
            }
        }

        protected void ThrowIfNotCreated() {
            if (!Created) {
                throw new BaseObjectNotCreatedException();
            }
        }
    }

    public abstract class BaseObject : Object, IBaseObject
    {
        public void Create() {
            if (Created) {
                return;
            }
            try {
                OnCreate();
            }
            finally {
                Created = true;
                Destroyed = false;
            }
        }

        protected abstract void OnCreate();
    }

    public abstract class BaseObject<T1> : Object, IBaseObject<T1>
    {
        public void Create(T1 arg1) {
            try {
                OnCreate(arg1);
            }
            finally {
                Created = true;
                Destroyed = false;
            }
        }

        protected abstract void OnCreate(T1 arg1);
    }

    public abstract class BaseObject<T1, T2> : Object, IBaseObject<T1, T2>
    {
        public void Create(T1 arg1, T2 arg2) {
            try {
                OnCreate(arg1, arg2);
            }
            finally {
                Created = true;
                Destroyed = false;
            }
        }

        protected abstract void OnCreate(T1 arg1, T2 arg2);
    }

    public abstract class BaseObject<T1, T2, T3> : Object, IBaseObject<T1, T2, T3>
    {
        public void Create(T1 arg1, T2 arg2, T3 arg3) {
            try {
                OnCreate(arg1, arg2, arg3);
            }
            finally {
                Created = true;
                Destroyed = false;
            }
        }

        protected abstract void OnCreate(T1 arg1, T2 arg2, T3 arg3);
    }

    public abstract class BaseObject<T1, T2, T3, T4> : Object, IBaseObject<T1, T2, T3, T4>
    {
        public void Create(T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            try {
                OnCreate(arg1, arg2, arg3, arg4);
            }
            finally {
                Created = true;
                Destroyed = false;
            }
        }

        protected abstract void OnCreate(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    }

    public abstract class BaseObject<T1, T2, T3, T4, T5> : Object, IBaseObject<T1, T2, T3, T4, T5>
    {
        public void Create(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            try {
                OnCreate(arg1, arg2, arg3, arg4, arg5);
            }
            finally {
                Created = true;
                Destroyed = false;
            }
        }

        protected abstract void OnCreate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    }
}