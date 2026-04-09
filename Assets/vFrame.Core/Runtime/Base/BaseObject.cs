// ------------------------------------------------------------
//         File: BaseObject.cs
//        Brief: Abstract base classes providing create/destroy lifecycle management
//                 with ownership tracking via ILifetime.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2016-07-29 11:01:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;


namespace vFrame.Core
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

        /// <summary>
        /// Releases all resources by invoking <see cref="Destroy"/>.
        /// </summary>
        public void Dispose() {
            Destroy();
        }

        /// <summary>
        /// Override to perform custom teardown logic when the object is destroyed.
        /// </summary>
        protected abstract void OnDestroy();

        /// <summary>
        /// Gets the lazy-initialized lifetime scope used to track owned resources.
        /// Throws if the object has already been destroyed.
        /// </summary>
        /// <exception cref="BaseObjectDestroyedException">Thrown when the object is already destroyed.</exception>
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
        /// <param name="destroyable">The destroyable to own.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="destroyable"/> is null.</exception>
        protected void Own(IDestroyable destroyable) {
            ThrowHelper.ThrowIfNull(destroyable, nameof(destroyable));
            Lifetime.Add(destroyable);
        }

        /// <summary>
        /// Registers an owned cleanup action that should run when this object is destroyed.
        /// </summary>
        /// <param name="action">The cleanup action to own.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
        protected void Own(Action action) {
            ThrowHelper.ThrowIfNull(action, nameof(action));
            Lifetime.Add(action);
        }

        /// <summary>
        /// Binds a destroyable to this object's lifetime without implying direct field ownership.
        /// Use this for lifetime-bound resources that should end with the object.
        /// </summary>
        /// <param name="destroyable">The destroyable to bind.</param>
        protected internal void OwnLifetime(IDestroyable destroyable) {
            Own(destroyable);
        }

        /// <summary>
        /// Binds a cleanup action to this object's lifetime without introducing a separate scope model.
        /// </summary>
        /// <param name="action">The cleanup action to bind.</param>
        protected internal void OwnLifetime(Action action) {
            Own(action);
        }

        /// <summary>
        /// Throws <see cref="BaseObjectDestroyedException"/> if the object has been destroyed.
        /// </summary>
        /// <exception cref="BaseObjectDestroyedException">Thrown when the object is already destroyed.</exception>
        protected void ThrowIfDestroyed() {
            if (Destroyed) {
                throw new BaseObjectDestroyedException();
            }
        }

        /// <summary>
        /// Throws <see cref="BaseObjectNotCreatedException"/> if the object has not been created.
        /// </summary>
        /// <exception cref="BaseObjectNotCreatedException">Thrown when the object has not been created.</exception>
        protected void ThrowIfNotCreated() {
            if (!Created) {
                throw new BaseObjectNotCreatedException();
            }
        }

        /// <summary>
        /// Throws if the object has been destroyed or has not yet been created.
        /// </summary>
        /// <exception cref="BaseObjectDestroyedException">Thrown when the object is already destroyed.</exception>
        /// <exception cref="BaseObjectNotCreatedException">Thrown when the object has not been created.</exception>
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

        /// <summary>
        /// Override to perform custom initialization logic when the object is created.
        /// </summary>
        protected abstract void OnCreate();
    }

    public abstract class BaseObject<T1> : Object, IBaseObject<T1>
    {
        /// <summary>
        /// Transitions the object into its usable state once with one argument.
        /// Destroyed instances stay terminal.
        /// </summary>
        /// <param name="arg1">The first creation argument.</param>
        public void Create(T1 arg1) {
            ThrowIfDestroyed();

            if (Created) {
                return;
            }

            OnCreate(arg1);
            Created = true;
            Destroyed = false;
        }

        /// <summary>
        /// Override to perform custom initialization logic when the object is created with one argument.
        /// </summary>
        /// <param name="arg1">The first creation argument.</param>
        protected abstract void OnCreate(T1 arg1);
    }

    public abstract class BaseObject<T1, T2> : Object, IBaseObject<T1, T2>
    {
        /// <summary>
        /// Transitions the object into its usable state once with two arguments.
        /// Destroyed instances stay terminal.
        /// </summary>
        /// <param name="arg1">The first creation argument.</param>
        /// <param name="arg2">The second creation argument.</param>
        public void Create(T1 arg1, T2 arg2) {
            ThrowIfDestroyed();

            if (Created) {
                return;
            }

            OnCreate(arg1, arg2);
            Created = true;
            Destroyed = false;
        }

        /// <summary>
        /// Override to perform custom initialization logic when the object is created with two arguments.
        /// </summary>
        /// <param name="arg1">The first creation argument.</param>
        /// <param name="arg2">The second creation argument.</param>
        protected abstract void OnCreate(T1 arg1, T2 arg2);
    }

    public abstract class BaseObject<T1, T2, T3> : Object, IBaseObject<T1, T2, T3>
    {
        /// <summary>
        /// Transitions the object into its usable state once with three arguments.
        /// Destroyed instances stay terminal.
        /// </summary>
        /// <param name="arg1">The first creation argument.</param>
        /// <param name="arg2">The second creation argument.</param>
        /// <param name="arg3">The third creation argument.</param>
        public void Create(T1 arg1, T2 arg2, T3 arg3) {
            ThrowIfDestroyed();

            if (Created) {
                return;
            }

            OnCreate(arg1, arg2, arg3);
            Created = true;
            Destroyed = false;
        }

        /// <summary>
        /// Override to perform custom initialization logic when the object is created with three arguments.
        /// </summary>
        /// <param name="arg1">The first creation argument.</param>
        /// <param name="arg2">The second creation argument.</param>
        /// <param name="arg3">The third creation argument.</param>
        protected abstract void OnCreate(T1 arg1, T2 arg2, T3 arg3);
    }

    public abstract class BaseObject<T1, T2, T3, T4> : Object, IBaseObject<T1, T2, T3, T4>
    {
        /// <summary>
        /// Transitions the object into its usable state once with four arguments.
        /// Destroyed instances stay terminal.
        /// </summary>
        /// <param name="arg1">The first creation argument.</param>
        /// <param name="arg2">The second creation argument.</param>
        /// <param name="arg3">The third creation argument.</param>
        /// <param name="arg4">The fourth creation argument.</param>
        public void Create(T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            ThrowIfDestroyed();

            if (Created) {
                return;
            }

            OnCreate(arg1, arg2, arg3, arg4);
            Created = true;
            Destroyed = false;
        }

        /// <summary>
        /// Override to perform custom initialization logic when the object is created with four arguments.
        /// </summary>
        /// <param name="arg1">The first creation argument.</param>
        /// <param name="arg2">The second creation argument.</param>
        /// <param name="arg3">The third creation argument.</param>
        /// <param name="arg4">The fourth creation argument.</param>
        protected abstract void OnCreate(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    }

    public abstract class BaseObject<T1, T2, T3, T4, T5> : Object, IBaseObject<T1, T2, T3, T4, T5>
    {
        /// <summary>
        /// Transitions the object into its usable state once with five arguments.
        /// Destroyed instances stay terminal.
        /// </summary>
        /// <param name="arg1">The first creation argument.</param>
        /// <param name="arg2">The second creation argument.</param>
        /// <param name="arg3">The third creation argument.</param>
        /// <param name="arg4">The fourth creation argument.</param>
        /// <param name="arg5">The fifth creation argument.</param>
        public void Create(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            ThrowIfDestroyed();

            if (Created) {
                return;
            }

            OnCreate(arg1, arg2, arg3, arg4, arg5);
            Created = true;
            Destroyed = false;
        }

        /// <summary>
        /// Override to perform custom initialization logic when the object is created with five arguments.
        /// </summary>
        /// <param name="arg1">The first creation argument.</param>
        /// <param name="arg2">The second creation argument.</param>
        /// <param name="arg3">The third creation argument.</param>
        /// <param name="arg4">The fourth creation argument.</param>
        /// <param name="arg5">The fifth creation argument.</param>
        protected abstract void OnCreate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    }
}
