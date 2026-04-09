//------------------------------------------------------------
//        File:  IAsyncRequest.cs
//       Brief:  Async request state enum and contract.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2024-3-19 20:42
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System;
using System.Collections;
using vFrame.Core;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Describes the current state of an async request.
    /// </summary>
    public enum AsyncState
    {
        /// <summary>
        /// The request has been created but not yet started.
        /// </summary>
        NotStarted,

        /// <summary>
        /// The request is currently being processed.
        /// </summary>
        Processing,

        /// <summary>
        /// The request has completed successfully.
        /// </summary>
        Finished,

        /// <summary>
        /// The request has encountered an error.
        /// </summary>
        Error,
    }

    /// <summary>
    /// Defines the contract for frame-driven async requests.
    /// </summary>
    public interface IAsyncRequest : IEnumerator, IBaseObject
    {
        /// <summary>
        /// Starts the request, transitioning from NotStarted to Processing.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the request, resetting it to NotStarted.
        /// </summary>
        void Stop();

        /// <summary>
        /// Advances the request by one frame while in the Processing state.
        /// </summary>
        void Update();

        /// <summary>
        /// Gets the current state of the request.
        /// </summary>
        AsyncState State { get; }

        /// <summary>
        /// Indicates whether the request has finished successfully.
        /// </summary>
        bool IsDone { get; }

        /// <summary>
        /// Indicates whether the request has encountered an error.
        /// </summary>
        bool IsError { get; }

        /// <summary>
        /// Gets the current progress of the request, from 0 to 1.
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// Raised when the request finishes successfully.
        /// </summary>
        event Action OnFinish;

        /// <summary>
        /// Raised when the request encounters an error.
        /// </summary>
        event Action OnError;
    }
}
