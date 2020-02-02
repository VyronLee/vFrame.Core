//------------------------------------------------------------
//        File:  CoroutineTask.cs
//       Brief:  Coroutine task
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-09-08 22:11
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using System;
using System.Collections;

namespace vFrame.Core.Coroutine
{
    [Serializable]
    internal struct CoroutineTask
    {
        public int Handle;
        public IEnumerator Task;
        public int RunnerId;
    }
}