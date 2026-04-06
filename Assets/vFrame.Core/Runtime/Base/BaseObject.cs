//------------------------------------------------------------
//       @file  BaseObject.cs
//      @brief  Base object class.
//
//     @author  VyronLee, lwz_jz@hotmail.com
//
//   @internal
//     Created  2016-07-29 11:01
//   Copyright  Copyright (c) 2024, VyronLee
//============================================================

using System;
using vFrame.Core.Exceptions;

namespace vFrame.Core.Base
{
    public abstract class Object : IDestroyable
    {
        private ILifetime _lifetime;

        /// <summary>
        /// Gets whether the instance has successfully completed its one-shot create transition.
        /// </summary>
        public bool Created { get; protected set; }

        /// <summary>
        /// Gets whether the instance has crossed its terminal destroy boundary.
        /// Destroyed instances do not support recreation.
        /// </summary>
        public bool Destroyed { get; protected set; }

        private bool Destroying { get; set; }

        /// <summary>
        /// Executes the unified, terminal teardown boundary for owned state and custom cleanup.
        /// </summary>
        public void Destroy() {
            if (Destroyed || Destroying) {
                return;
            }

            Exception destroyException = null;

            try {
                Destroying = true;
                OnDestroy();
            }
            catch (Exception exception) {
                destroyException = exception;
            }
            finally {
                try {
                    _lifetime?.Destroy();
                }
                finally {
                    _lifetime = null;
                    Destroying = false;
                    Destroyed = true;
                    Created = false;
                }
            }

            if (destroyException != null) {
                throw destroyException;
            }
        }

        public void Dispose() {
            Destroy();
        }

        protected abstract void OnDestroy();

        protected ILifetime Lifetime {
            get {
                ThrowIfDestroyed();

                _lifetime ??= new Lifetime();
                return _lifetime;
            }
        }

        /// <summary>
        /// Registers an owned destroyable that should terminate when this object crosses its
        /// destroy boundary.
        /// </summary>
        protected void Own(IDestroyable destroyable) {
            ThrowHelper.ThrowIfNull(destroyable, nameof(destroyable));
            Lifetime.Add(destroyable);
        }

        /// <summary>
        /// Registers an owned cleanup action that should run when this object is destroyed.
        /// </summary>
        protected void Own(Action action) {
            ThrowHelper.ThrowIfNull(action, nameof(action));
            Lifetime.Add(action);
        }

        /// <summary>
        /// Binds a destroyable to this object's lifetime without implying direct field ownership.
        /// Use this for lifetime-bound resources that should end with the object.
        /// </summary>
        protected internal void OwnLifetime(IDestroyable destroyable) {
            Own(destroyable);
        }

        /// <summary>
        /// Binds a cleanup action to this object's lifetime without introducing a separate scope model.
        /// </summary>
        protected internal void OwnLifetime(Action action) {
            Own(action);
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

        protected void ThrowIfNotCreatedOrDestroyed() {
            ThrowIfDestroyed();
            ThrowIfNotCreated();
        }
    }

    public abstract class BaseObject : Object, IBaseObject
    {
        /// <summary>
        /// Transitions the object into its usable state once. Destroyed instances stay terminal.
        /// </summary>
        public void Create() {
            ThrowIfDestroyed();

            if (Created) {
                return;
            }

            OnCreate();
            Created = true;
            Destroyed = false;
        }

        protected abstract void OnCreate();
    }

    public abstract class BaseObject<T1> : Object, IBaseObject<T1>
    {
        /// <summary>
        /// Transitions the object into its usable state once. Destroyed instances stay terminal.
        /// </summary>
        public void Create(T1 arg1) {
            ThrowIfDestroyed();

            if (Created) {
                return;
            }

            OnCreate(arg1);
            Created = true;
            Destroyed = false;
        }

        protected abstract void OnCreate(T1 arg1);
    }

    public abstract class BaseObject<T1, T2> : Object, IBaseObject<T1, T2>
    {
        /// <summary>
        /// Transitions the object into its usable state once. Destroyed instances stay terminal.
        /// </summary>
        public void Create(T1 arg1, T2 arg2) {
            ThrowIfDestroyed();

            if (Created) {
                return;
            }

            OnCreate(arg1, arg2);
            Created = true;
            Destroyed = false;
        }

        protected abstract void OnCreate(T1 arg1, T2 arg2);
    }

    public abstract class BaseObject<T1, T2, T3> : Object, IBaseObject<T1, T2, T3>
    {
        /// <summary>
        /// Transitions the object into its usable state once. Destroyed instances stay terminal.
        /// </summary>
        public void Create(T1 arg1, T2 arg2, T3 arg3) {
            ThrowIfDestroyed();

            if (Created) {
                return;
            }

            OnCreate(arg1, arg2, arg3);
            Created = true;
            Destroyed = false;
        }

        protected abstract void OnCreate(T1 arg1, T2 arg2, T3 arg3);
    }

    public abstract class BaseObject<T1, T2, T3, T4> : Object, IBaseObject<T1, T2, T3, T4>
    {
        /// <summary>
        /// Transitions the object into its usable state once. Destroyed instances stay terminal.
        /// </summary>
        public void Create(T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            ThrowIfDestroyed();

            if (Created) {
                return;
            }

            OnCreate(arg1, arg2, arg3, arg4);
            Created = true;
            Destroyed = false;
        }

        protected abstract void OnCreate(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    }

    public abstract class BaseObject<T1, T2, T3, T4, T5> : Object, IBaseObject<T1, T2, T3, T4, T5>
    {
        /// <summary>
        /// Transitions the object into its usable state once. Destroyed instances stay terminal.
        /// </summary>
        public void Create(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            ThrowIfDestroyed();

            if (Created) {
                return;
            }

            OnCreate(arg1, arg2, arg3, arg4, arg5);
            Created = true;
            Destroyed = false;
        }

        protected abstract void OnCreate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    }
}
