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
        // Reusable per-frame buffers: completed requests land here and are cleared at the
        // end of Update(), so steady-state frames allocate zero Lists (C15). Touched only
        // within a single Update() call (main thread) — the lock guards _requests, not these.
        private readonly List<IAsyncRequest> _finished = new List<IAsyncRequest>();
        private readonly List<IAsyncRequest> _errored = new List<IAsyncRequest>();
        private readonly object _lock = new object();

        private static volatile AsyncRequestCtrl _shared;
        private static readonly object _sharedLock = new object();

        /// <summary>
        /// Gets the shared singleton instance, creating and self-driving it on first access.
        /// Thread-safe via double-check locking.
        /// </summary>
        public static AsyncRequestCtrl Shared {
            get {
                if (_shared != null) {
                    return _shared;
                }
                lock (_sharedLock) {
                    if (_shared != null) {
                        return _shared;
                    }
                    _shared = new AsyncRequestCtrl();
                    _shared.Create();
                    _shared.SelfDrive();
                    return _shared;
                }
            }
        }

        protected override void OnCreate() {
            _requests = new List<IAsyncRequest>(64);
        }

        protected override void OnDestroy() {
            lock (_lock) {
                if (_requests != null) {
                    _requests.Clear();
                    _requests = null;
                }
            }

            _finished.Clear();
            _errored.Clear();

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
        /// Uses deferred cleanup to avoid modifying the list inside callbacks.
        /// Completed requests are collected into reusable per-frame buffers that are
        /// cleared at the end, so a steady-state frame allocates no Lists (C15).
        /// </summary>
        public void Update() {
            ThrowIfNotCreatedOrDestroyed();

            // Phase 1: drive all requests, collect completed ones into the reusable buffers
            lock (_lock) {
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
                            // Re-check state after update — may have transitioned
                            if (request.State == AsyncState.Finished) {
                                _finished.Add(request);
                                _requests.RemoveAt(i);
                            }
                            else if (request.State == AsyncState.Error) {
                                _errored.Add(request);
                                _requests.RemoveAt(i);
                            }
                            break;
                        case AsyncState.Finished:
                            _finished.Add(request);
                            _requests.RemoveAt(i);
                            break;
                        case AsyncState.Error:
                            _errored.Add(request);
                            _requests.RemoveAt(i);
                            break;
                    }
                }
            }

            // Phase 2: fire callbacks outside the lock
            // Callbacks may call AddRequest/RemoveRequest — safe because we're not iterating _requests
            if (_finished.Count > 0) {
                for (var i = 0; i < _finished.Count; i++) {
                    OnRequestFinish?.Invoke(_finished[i]);
                }
            }
            if (_errored.Count > 0) {
                for (var i = 0; i < _errored.Count; i++) {
                    OnRequestError?.Invoke(_errored[i]);
                }
            }

            // Release references and reset the buffers for the next frame (C15).
            _finished.Clear();
            _errored.Clear();
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
        /// Inserted in priority order (lower value = higher priority = processed first).
        /// </summary>
        public void AddRequest(IAsyncRequest request) {
            ThrowIfNotCreatedOrDestroyed();
            ThrowHelper.ThrowIfNull(request, nameof(request));
            lock (_lock) {
                var priority = request.Priority;
                var index = _requests.Count;
                for (var i = 0; i < _requests.Count; i++) {
                    if (_requests[i].Priority < priority) {
                        index = i;
                        break;
                    }
                }
                _requests.Insert(index, request);
            }
        }

        /// <summary>
        /// Removes a previously registered async request.
        /// </summary>
        public void RemoveRequest(IAsyncRequest request) {
            ThrowIfNotCreatedOrDestroyed();
            ThrowHelper.ThrowIfNull(request, nameof(request));
            lock (_lock) {
                _requests.Remove(request);
            }
        }

        /// <summary>
        /// Gets the number of currently registered requests.
        /// </summary>
        public int RequestCount {
            get {
                lock (_lock) {
                    return _requests?.Count ?? 0;
                }
            }
        }

        /// <summary>
        /// Stops and removes all registered requests.
        /// </summary>
        public void CancelAll() {
            ThrowIfNotCreatedOrDestroyed();
            lock (_lock) {
                for (var i = _requests.Count - 1; i >= 0; i--) {
                    _requests[i].Stop();
                }
                _requests.Clear();
            }
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
