// ------------------------------------------------------------
//         File: BaseObjectCreateAbility.cs
//        Brief: BaseObjectCreateAbility.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-18 17:40
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core.Base
{
    public abstract class CreateAbility<T> : BaseObject where T : BaseObject
    {
        public new static T Create() {
            var ret = Activator.CreateInstance<T>();
            ret.Create();
            return ret;
        }
    }

    public abstract class CreateAbility<T, T1> : BaseObject<T1> where T : BaseObject<T1>
    {
        public new static T Create(T1 arg1) {
            var ret = Activator.CreateInstance<T>();
            ret.Create(arg1);
            return ret;
        }
    }

    public abstract class CreateAbility<T, T1, T2> : BaseObject<T1, T2> where T : BaseObject<T1, T2>
    {
        public new static T Create(T1 arg1, T2 arg2) {
            var ret = Activator.CreateInstance<T>();
            ret.Create(arg1, arg2);
            return ret;
        }
    }

    public abstract class CreateAbility<T, T1, T2, T3> : BaseObject<T1, T2, T3> where T : BaseObject<T1, T2, T3>
    {
        public new static T Create(T1 arg1, T2 arg2, T3 arg3) {
            var ret = Activator.CreateInstance<T>();
            ret.Create(arg1, arg2, arg3);
            return ret;
        }
    }

    public abstract class CreateAbility<T, T1, T2, T3, T4> : BaseObject<T1, T2, T3, T4>
        where T : BaseObject<T1, T2, T3, T4>
    {
        public new static T Create(T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            var ret = Activator.CreateInstance<T>();
            ret.Create(arg1, arg2, arg3, arg4);
            return ret;
        }
    }

    public abstract class CreateAbility<T, T1, T2, T3, T4, T5> : BaseObject<T1, T2, T3, T4, T5>
        where T : BaseObject<T1, T2, T3, T4, T5>
    {
        public new static T Create(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            var ret = Activator.CreateInstance<T>();
            ret.Create(arg1, arg2, arg3, arg4, arg5);
            return ret;
        }
    }
}