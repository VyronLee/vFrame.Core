// ------------------------------------------------------------
//         File: IBaseObject.cs
//        Brief: Base object interface combining creatable and destroyable lifecycle.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 16:04:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    /// <summary>
    ///     Defines a base object with parameterless creation and destruction lifecycle.
    /// </summary>
    public interface IBaseObject : ICreatable, IDestroyable
    { }

    /// <summary>
    ///     Defines a base object with single-parameter creation and destruction lifecycle.
    /// </summary>
    public interface IBaseObject<in T1> : ICreatable<T1>, IDestroyable
    { }

    /// <summary>
    ///     Defines a base object with two-parameter creation and destruction lifecycle.
    /// </summary>
    public interface IBaseObject<in T1, in T2> : ICreatable<T1, T2>, IDestroyable
    { }

    /// <summary>
    ///     Defines a base object with three-parameter creation and destruction lifecycle.
    /// </summary>
    public interface IBaseObject<in T1, in T2, in T3> : ICreatable<T1, T2, T3>, IDestroyable
    { }

    /// <summary>
    ///     Defines a base object with four-parameter creation and destruction lifecycle.
    /// </summary>
    public interface IBaseObject<in T1, in T2, in T3, in T4> : ICreatable<T1, T2, T3, T4>, IDestroyable
    { }

    /// <summary>
    ///     Defines a base object with five-parameter creation and destruction lifecycle.
    /// </summary>
    public interface IBaseObject<in T1, in T2, in T3, in T4, in T5> : ICreatable<T1, T2, T3, T4, T5>, IDestroyable
    { }
}