//------------------------------------------------------------
//        File:  IAsyncRequestCtrl.cs
//       Brief:  Contract for async request lifecycle controllers.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2024-3-19 20:44
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System;
using vFrame.Core;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Defines the contract for controllers that manage async request lifecycle and per-frame updates.
    /// </summary>
    public interface IAsyncRequestCtrl : IBaseObject
    {
        /// <summary>
        /// Advances all registered requests by one frame.
        /// </summary>
        void Update();

        /// <summary>
        /// Attaches a Unity MonoBehaviour driver for automatic per-frame ticking.
        /// </summary>
        void SelfDrive();

        /// <summary>
        /// Creates a new async request of the specified generic type.
        /// </summary>
        T CreateRequest<T>() where T : IAsyncRequest;

        /// <summary>
        /// Creates a new async request of the specified runtime type.
        /// </summary>
        IAsyncRequest CreateRequest(Type type);

        /// <summary>
        /// Registers an async request for lifecycle management.
        /// </summary>
        void AddRequest(IAsyncRequest request);

        /// <summary>
        /// Raised when a request completes successfully.
        /// </summary>
        event Action<IAsyncRequest> OnRequestFinish;

        /// <summary>
        /// Raised when a request encounters an error.
        /// </summary>
        event Action<IAsyncRequest> OnRequestError;
    }
}
