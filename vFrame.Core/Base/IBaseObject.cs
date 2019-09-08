//------------------------------------------------------------
//        File:  IBaseObject.cs
//       Brief:  Base object interface.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-09-08 23:31
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================
namespace vFrame.Core.Base
{
    public interface IBaseObject
    {
        void Create();
        void Destroy();
    }

    public interface IBaseObject<in T1> : IBaseObject
    {
        void Create(T1 arg1);
        void Destroy();
    }

    public interface IBaseObject<in T1, in T2> : IBaseObject
    {
        void Create(T1 arg1, T2 arg2);
        void Destroy();
    }

    public interface IBaseObject<in T1, in T2, in T3> : IBaseObject
    {
        void Create(T1 arg1, T2 arg2, T3 arg3);
        void Destroy();
    }
}