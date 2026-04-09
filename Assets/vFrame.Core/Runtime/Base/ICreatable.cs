// ------------------------------------------------------------
//         File: ICreatable.cs
//        Brief: Interface defining a creatable object lifecycle.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 23:31:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    /// <summary>
    /// Defines a parameterless creatable object lifecycle.
    /// </summary>
    public interface ICreatable
    {
        /// <summary>
        /// Gets whether the object has been created.
        /// </summary>
        bool Created { get; }

        /// <summary>
        /// Creates the object instance.
        /// </summary>
        void Create();
    }

    /// <summary>
    /// Defines a single-parameter creatable object lifecycle.
    /// </summary>
    public interface ICreatable<in T1>
    {
        /// <summary>
        /// Gets whether the object has been created.
        /// </summary>
        bool Created { get; }

        /// <summary>
        /// Creates the object instance with one argument.
        /// </summary>
        void Create(T1 arg1);
    }

    /// <summary>
    /// Defines a two-parameter creatable object lifecycle.
    /// </summary>
    public interface ICreatable<in T1, in T2>
    {
        /// <summary>
        /// Gets whether the object has been created.
        /// </summary>
        bool Created { get; }

        /// <summary>
        /// Creates the object instance with two arguments.
        /// </summary>
        void Create(T1 arg1, T2 arg2);
    }

    /// <summary>
    /// Defines a three-parameter creatable object lifecycle.
    /// </summary>
    public interface ICreatable<in T1, in T2, in T3>
    {
        /// <summary>
        /// Gets whether the object has been created.
        /// </summary>
        bool Created { get; }

        /// <summary>
        /// Creates the object instance with three arguments.
        /// </summary>
        void Create(T1 arg1, T2 arg2, T3 arg3);
    }

    /// <summary>
    /// Defines a four-parameter creatable object lifecycle.
    /// </summary>
    public interface ICreatable<in T1, in T2, in T3, in T4>
    {
        /// <summary>
        /// Gets whether the object has been created.
        /// </summary>
        bool Created { get; }

        /// <summary>
        /// Creates the object instance with four arguments.
        /// </summary>
        void Create(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    }

    /// <summary>
    /// Defines a five-parameter creatable object lifecycle.
    /// </summary>
    public interface ICreatable<in T1, in T2, in T3, in T4, in T5>
    {
        /// <summary>
        /// Gets whether the object has been created.
        /// </summary>
        bool Created { get; }

        /// <summary>
        /// Creates the object instance with five arguments.
        /// </summary>
        void Create(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    }
}
