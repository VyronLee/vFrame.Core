//------------------------------------------------------------
//        File:  AsyncRequest.cs
//       Brief:  Base class for frame-driven async requests.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2024-3-19 20:42
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using System;
using System.Threading;
using UnityEngine;
using vFrame.Core;

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Abstract base class for frame-driven async requests with start/stop/update lifecycle.
    /// </summary>
    public abstract class AsyncRequest : BaseObject, IAsyncRequest
    {
        /// <summary>
        /// Gets the current state of the request.
        /// </summary>
        public AsyncState State { get; private set; }

        /// <summary>
        /// Gets the exception that caused the error state, or null if no error.
        /// </summary>
        public Exception LastError { get; private set; }

        /// <summary>
        /// Registers this request with the shared controller for automatic per-frame updates.
        /// </summary>
        public void WithSharedCtrl() {
            AsyncRequestCtrl.Shared.AddRequest(this);
        }

        /// <summary>
        /// Starts the request, transitioning from NotStarted to Processing.
        /// </summary>
        public void Start() {
            if (State != AsyncState.NotStarted) {
                return;
            }
            _elapsedTime = 0f;
            LastError = null;
            State = AsyncState.Processing;
            OnStart();
        }

        /// <summary>
        /// Stops the request, resetting it to NotStarted.
        /// Only valid from Processing state to prevent re-using finished requests.
        /// </summary>
        public void Stop() {
            if (State != AsyncState.Processing) {
                return;
            }
            State = AsyncState.NotStarted;
            LastError = null;
            OnStop();
        }

        /// <summary>
        /// Advances the request by one frame while in the Processing state.
        /// Checks cancellation and timeout before delegating to OnUpdate.
        /// </summary>
        public void Update() {
            if (State != AsyncState.Processing) {
                return;
            }
            if (CancellationToken.IsCancellationRequested) {
                SetError(new OperationCanceledException("Async request was cancelled."));
                return;
            }
            _elapsedTime += Time.deltaTime;
            if (TimeoutSeconds > 0f && _elapsedTime >= TimeoutSeconds) {
                SetError(new TimeoutException(
                    $"Async request timed out after {TimeoutSeconds}s."));
                return;
            }
            OnUpdate();
        }

        /// <summary>
        /// Indicates whether the request has finished successfully.
        /// </summary>
        public bool IsDone => State == AsyncState.Finished;

        /// <summary>
        /// Indicates whether the request has encountered an error.
        /// </summary>
        public bool IsError => State == AsyncState.Error;

        /// <summary>
        /// Gets the cancellation token associated with this request.
        /// </summary>
        public CancellationToken CancellationToken { get; private set; }

        /// <summary>
        /// Gets the timeout duration in seconds. A value of 0 means no timeout.
        /// </summary>
        public float TimeoutSeconds { get; private set; } = 0f;

        /// <summary>
        /// Gets the priority of the request. Lower values indicate higher priority.
        /// </summary>
        public int Priority { get; protected set; } = 0;

        /// <summary>
        /// Sets the cancellation token for this request.
        /// </summary>
        public void SetCancellationToken(CancellationToken token) {
            CancellationToken = token;
        }

        /// <summary>
        /// Sets the timeout duration in seconds. A value of 0 disables timeout.
        /// </summary>
        public void SetTimeout(float timeoutSeconds) {
            TimeoutSeconds = timeoutSeconds;
        }

        /// <summary>
        /// Sets the priority of the request. Lower values indicate higher priority.
        /// </summary>
        public void SetPriority(int priority) {
            Priority = priority;
        }

        /// <summary>
        /// Gets the current progress of the request, from 0 to 1.
        /// </summary>
        public abstract float Progress { get; }

        /// <summary>
        /// Raised when the request finishes successfully.
        /// </summary>
        public event Action OnFinish;

        /// <summary>
        /// Raised when the request encounters an error.
        /// </summary>
        public event Action OnError;

        /// <summary>
        /// Advances the coroutine; returns true while the request is still active.
        /// </summary>
        public bool MoveNext() {
            return !IsDone && !IsError;
        }

        /// <summary>
        /// Resets the coroutine by stopping the request.
        /// </summary>
        public void Reset() {
            Stop();
        }

        /// <summary>
        /// Gets the current coroutine element, always null for async requests.
        /// </summary>
        public object Current => null;

        private float _elapsedTime;

        protected override void OnCreate() {

        }

        protected override void OnDestroy() {
            // Notify error listeners if the request is still processing
            if (State == AsyncState.Processing) {
                SetError(new ObjectDisposedException(GetType().Name,
                    "Async request was destroyed while still processing."));
            }
            Stop();
        }

        /// <summary>
        /// Transitions the request to the Error state and raises the OnError event.
        /// </summary>
        protected void Abort() {
            if (State == AsyncState.Error) {
                return;
            }
            SetError(null);
        }

        /// <summary>
        /// Transitions the request to the Error state with a specific exception.
        /// </summary>
        protected void Abort(Exception exception) {
            if (State == AsyncState.Error) {
                return;
            }
            SetError(exception);
        }

        /// <summary>
        /// Transitions the request to the Finished state and raises the OnFinish event.
        /// </summary>
        protected void Finish() {
            if (State == AsyncState.Finished) {
                return;
            }
            State = AsyncState.Finished;
            LastError = null;
            OnFinish?.Invoke();
        }

        /// <summary>
        /// Throws if the request has not reached the Finished state.
        /// </summary>
        protected void ThrowIfNotFinished() {
            if (State != AsyncState.Finished) {
                throw new AsyncRequestNotFinishedException();
            }
        }

        /// <summary>
        /// Called when the request starts.
        /// </summary>
        protected abstract void OnStart();

        /// <summary>
        /// Called when the request is stopped.
        /// </summary>
        protected abstract void OnStop();

        /// <summary>
        /// Called each frame while the request is processing.
        /// </summary>
        protected abstract void OnUpdate();

        /// <summary>
        /// Sets the error state with an optional exception and raises OnError.
        /// </summary>
        private void SetError(Exception exception) {
            if (State == AsyncState.Error) {
                return;
            }
            State = AsyncState.Error;
            LastError = exception;
            OnError?.Invoke();
        }
    }
}
