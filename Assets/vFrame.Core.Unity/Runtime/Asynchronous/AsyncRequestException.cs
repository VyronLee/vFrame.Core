//------------------------------------------------------------
//        File:  AsyncRequestException.cs
//       Brief:  Exception types for async request failures.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2024-3-19 20:42
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Base exception for async request errors.
    /// </summary>
    public class AsyncRequestException : Exception { }

    /// <summary>
    /// Thrown when an operation requires a finished request but the request has not yet completed.
    /// </summary>
    public class AsyncRequestNotFinishedException : AsyncRequestException { }

    /// <summary>
    /// Thrown when an async request type cannot be instantiated.
    /// </summary>
    public class AsyncRequestTypeErrorException : AsyncRequestException { }
}
