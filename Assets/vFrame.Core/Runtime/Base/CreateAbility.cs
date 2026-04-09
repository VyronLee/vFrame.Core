// ------------------------------------------------------------
//         File: CreateAbility.cs
//        Brief: Generic factory base classes that instantiate
//               derived BaseObject types via Activator.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 17:40:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    /// <summary>
    /// Factory base class for creating parameterless <typeparamref name="T"/> instances.
    /// </summary>
    /// <typeparam name="T">The concrete type to instantiate, must derive from <see cref="BaseObject"/>.</typeparam>
    public abstract class CreateAbility<T> : BaseObject where T : BaseObject
    {
        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/> via reflection and calls <see cref="BaseObject.Create"/>.
        /// </summary>
        /// <returns>A fully initialized instance of <typeparamref name="T"/>.</returns>
        public new static T Create() {
            var ret = Activator.CreateInstance<T>();
            ret.Create();
            return ret;
        }
    }

    /// <summary>
    /// Factory base class for creating <typeparamref name="T"/> instances with one constructor argument.
    /// </summary>
    /// <typeparam name="T">The concrete type to instantiate.</typeparam>
    /// <typeparam name="T1">The type of the first constructor argument.</typeparam>
    public abstract class CreateAbility<T, T1> : BaseObject<T1> where T : BaseObject<T1>
    {
        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/> with the specified argument.
        /// </summary>
        /// <param name="arg1">The first argument passed to <see cref="BaseObject{T1}.Create"/>.</param>
        /// <returns>A fully initialized instance of <typeparamref name="T"/>.</returns>
        public new static T Create(T1 arg1) {
            var ret = Activator.CreateInstance<T>();
            ret.Create(arg1);
            return ret;
        }
    }

    /// <summary>
    /// Factory base class for creating <typeparamref name="T"/> instances with two constructor arguments.
    /// </summary>
    /// <typeparam name="T">The concrete type to instantiate.</typeparam>
    /// <typeparam name="T1">The type of the first constructor argument.</typeparam>
    /// <typeparam name="T2">The type of the second constructor argument.</typeparam>
    public abstract class CreateAbility<T, T1, T2> : BaseObject<T1, T2> where T : BaseObject<T1, T2>
    {
        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/> with the specified arguments.
        /// </summary>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <returns>A fully initialized instance of <typeparamref name="T"/>.</returns>
        public new static T Create(T1 arg1, T2 arg2) {
            var ret = Activator.CreateInstance<T>();
            ret.Create(arg1, arg2);
            return ret;
        }
    }

    /// <summary>
    /// Factory base class for creating <typeparamref name="T"/> instances with three constructor arguments.
    /// </summary>
    /// <typeparam name="T">The concrete type to instantiate.</typeparam>
    /// <typeparam name="T1">The type of the first constructor argument.</typeparam>
    /// <typeparam name="T2">The type of the second constructor argument.</typeparam>
    /// <typeparam name="T3">The type of the third constructor argument.</typeparam>
    public abstract class CreateAbility<T, T1, T2, T3> : BaseObject<T1, T2, T3> where T : BaseObject<T1, T2, T3>
    {
        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/> with the specified arguments.
        /// </summary>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <returns>A fully initialized instance of <typeparamref name="T"/>.</returns>
        public new static T Create(T1 arg1, T2 arg2, T3 arg3) {
            var ret = Activator.CreateInstance<T>();
            ret.Create(arg1, arg2, arg3);
            return ret;
        }
    }

    /// <summary>
    /// Factory base class for creating <typeparamref name="T"/> instances with four constructor arguments.
    /// </summary>
    /// <typeparam name="T">The concrete type to instantiate.</typeparam>
    /// <typeparam name="T1">The type of the first constructor argument.</typeparam>
    /// <typeparam name="T2">The type of the second constructor argument.</typeparam>
    /// <typeparam name="T3">The type of the third constructor argument.</typeparam>
    /// <typeparam name="T4">The type of the fourth constructor argument.</typeparam>
    public abstract class CreateAbility<T, T1, T2, T3, T4> : BaseObject<T1, T2, T3, T4>
        where T : BaseObject<T1, T2, T3, T4>
    {
        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/> with the specified arguments.
        /// </summary>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="arg4">The fourth argument.</param>
        /// <returns>A fully initialized instance of <typeparamref name="T"/>.</returns>
        public new static T Create(T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            var ret = Activator.CreateInstance<T>();
            ret.Create(arg1, arg2, arg3, arg4);
            return ret;
        }
    }

    /// <summary>
    /// Factory base class for creating <typeparamref name="T"/> instances with five constructor arguments.
    /// </summary>
    /// <typeparam name="T">The concrete type to instantiate.</typeparam>
    /// <typeparam name="T1">The type of the first constructor argument.</typeparam>
    /// <typeparam name="T2">The type of the second constructor argument.</typeparam>
    /// <typeparam name="T3">The type of the third constructor argument.</typeparam>
    /// <typeparam name="T4">The type of the fourth constructor argument.</typeparam>
    /// <typeparam name="T5">The type of the fifth constructor argument.</typeparam>
    public abstract class CreateAbility<T, T1, T2, T3, T4, T5> : BaseObject<T1, T2, T3, T4, T5>
        where T : BaseObject<T1, T2, T3, T4, T5>
    {
        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/> with the specified arguments.
        /// </summary>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="arg4">The fourth argument.</param>
        /// <param name="arg5">The fifth argument.</param>
        /// <returns>A fully initialized instance of <typeparamref name="T"/>.</returns>
        public new static T Create(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            var ret = Activator.CreateInstance<T>();
            ret.Create(arg1, arg2, arg3, arg4, arg5);
            return ret;
        }
    }
}
