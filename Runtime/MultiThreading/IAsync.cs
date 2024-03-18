//------------------------------------------------------------
//        File:  IAsync.cs
//       Brief:  IAsync
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-02-15 20:05
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System.Collections;

namespace vFrame.Core.MultiThreading
{
    public interface IAsync : IEnumerator
    {
        bool IsDone { get; }
        float Progress { get; }
    }
}