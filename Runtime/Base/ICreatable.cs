//------------------------------------------------------------
//        File:  ICreatable.cs
//       Brief:  ICreatable interface.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-08 23:31
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

namespace vFrame.Core.Base
{
    public interface ICreatable
    {
        bool Created { get; }
        void Create();
    }

    public interface ICreatable<in T1>
    {
        bool Created { get; }
        void Create(T1 arg1);
    }

    public interface ICreatable<in T1, in T2>
    {
        bool Created { get; }
        void Create(T1 arg1, T2 arg2);
    }

    public interface ICreatable<in T1, in T2, in T3>
    {
        bool Created { get; }
        void Create(T1 arg1, T2 arg2, T3 arg3);
    }

    public interface ICreatable<in T1, in T2, in T3, in T4>
    {
        bool Created { get; }
        void Create(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    }

    public interface ICreatable<in T1, in T2, in T3, in T4, in T5>
    {
        bool Created { get; }
        void Create(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    }
}