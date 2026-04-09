//------------------------------------------------------------
//        File:  AsyncRequestCtrl.cs
//       Brief:  Controller that drives async request lifecycle and frame updates.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2024-3-19 20:42
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using vFrame.Core;
using vFrame.Core.Unity;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Manages the lifecycle and per-frame updates of registered async requests.
    /// </summary>
    public class AsyncRequestCtrl : BaseObject, IAsyncRequestCtrl
    {
        private List<IAsyncRequest> _requests;
        private UpdateDriver _driver;

        private static AsyncRequestCtrl _shared;

        /// <summary>
        /// Gets the shared singleton instance, creating and self-driving it on first access.
        /// </summary>
        public static AsyncRequestCtrl Shared {
            get {
                if (_shared != null) {
                    return _shared;
                }
                _shared = new AsyncRequestCtrl();
                _shared.Create();
                _shared.SelfDrive();
                return _shared;
            }
        }

        protected override void OnCreate() {
            _requests = new List<IAsyncRequest>(64);
        }

        protected override void OnDestroy() {
            _requests?.Clear();
            _requests = null;

            if (null != _driver) {
                _driver.gameObject.DestroyEx();
                _driver = null;
            }
        }

        /// <summary>
        /// Attaches a Unity MonoBehaviour update driver so requests are ticked automatically.
        /// </summary>
        public void SelfDrive() {
            if (null != _driver) {
                return;
            }
            _driver = new GameObject(nameof(AsyncRequestCtrl))
                .DontDestroyEx()
                .DontSaveAndHideEx()
                .AddComponent<UpdateDriver>();
            _driver.Ctrl = this;
        }

        /// <summary>
        /// Advances all registered requests by one frame, handling state transitions.
        /// </summary>
        public void Update() {
            ThrowIfNotCreatedOrDestroyed();

            for (var i = _requests.Count - 1; i >= 0; i--) {
                var request = _requests[i];
                if (request.Destroyed) {
                    _requests.RemoveAt(i);
                    continue;
                }
                switch (request.State) {
                    case AsyncState.NotStarted:
                        request.Start();
                        break;
                    case AsyncState.Processing:
                        request.Update();
                        break;
                    case AsyncState.Finished:
                        _requests.RemoveAt(i);
                        OnRequestFinish?.Invoke(request);
                        break;
                    case AsyncState.Error:
                        _requests.RemoveAt(i);
                        OnRequestError?.Invoke(request);
                        break;
                }
            }
        }

        /// <summary>
        /// Creates a new async request of the specified generic type.
        /// </summary>
        public T CreateRequest<T>() where T : IAsyncRequest {
            ThrowIfNotCreatedOrDestroyed();
            return (T) CreateRequest(typeof(T));
        }

        /// <summary>
        /// Creates a new async request of the specified runtime type.
        /// </summary>
        public IAsyncRequest CreateRequest(Type type) {
            ThrowIfNotCreatedOrDestroyed();
            if (!(Activator.CreateInstance(type) is AsyncRequest request)) {
                throw new AsyncRequestTypeErrorException();
            }
            request.Create();
            AddRequest(request);
            return request;
        }

        /// <summary>
        /// Registers an async request for lifecycle management.
        /// </summary>
        public void AddRequest(IAsyncRequest request) {
            ThrowIfNotCreatedOrDestroyed();
            ThrowHelper.ThrowIfNull(request, nameof(request));
            _requests.Add(request);
        }

        /// <summary>
        /// Removes a previously registered async request.
        /// </summary>
        public void RemoveRequest(IAsyncRequest request) {
            ThrowIfNotCreatedOrDestroyed();
            ThrowHelper.ThrowIfNull(request, nameof(request));
            _requests.Remove(request);
        }

        /// <summary>
        /// Raised when a request completes successfully.
        /// </summary>
        public event Action<IAsyncRequest> OnRequestFinish;

        /// <summary>
        /// Raised when a request encounters an error.
        /// </summary>
        public event Action<IAsyncRequest> OnRequestError;

        private class UpdateDriver : MonoBehaviour
        {
            public AsyncRequestCtrl Ctrl { get; set; }

            private void Update() {
                Ctrl?.Update();
            }
        }
    }
}
