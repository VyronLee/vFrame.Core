// ------------------------------------------------------------
//         File: ObjectNotReadyException.cs
//        Brief: Exception thrown when a pooled object is accessed before it is ready.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core.Unity
{
    public class ObjectNotReadyException : SpawnPoolException { }
}
