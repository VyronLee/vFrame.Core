// ------------------------------------------------------------
//         File: IBaseObject.cs
//        Brief: IBaseObject.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-18 16:4
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core.Base
{
    public interface IBaseObject : ICreatable, IDestroyable { }

    public interface IBaseObject<in T1> : ICreatable<T1>, IDestroyable { }

    public interface IBaseObject<in T1, in T2> : ICreatable<T1, T2>, IDestroyable { }

    public interface IBaseObject<in T1, in T2, in T3> : ICreatable<T1, T2, T3>, IDestroyable { }

    public interface IBaseObject<in T1, in T2, in T3, in T4> : ICreatable<T1, T2, T3, T4>, IDestroyable { }

    public interface IBaseObject<in T1, in T2, in T3, in T4, in T5> : ICreatable<T1, T2, T3, T4, T5>, IDestroyable { }
}