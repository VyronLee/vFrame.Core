// ------------------------------------------------------------
//         File: DownloadAgentWebClient.cs
//        Brief: Download agent implementation that uses
//                System.Net.WebClient for asynchronous file
//                downloads with progress tracking and timeout.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.ComponentModel;
using System.Net;

namespace vFrame.Core.Unity
{
    /// <summary>
    ///     <see cref="DownloadAgentBase" /> implementation backed by
    ///     <see cref="WebClient" /> for asynchronous file downloads.
    ///     Tracks download progress and enforces a configurable timeout.
    /// </summary>
    public class DownloadAgentWebClient : DownloadAgentBase
    {
        private readonly object _lockObj = new object();
        private bool _done;
        private ulong _downloadedSize;
        private Exception _error;

        private ulong _lastDownloadedSize;
        private float _progress;
        private float _progressCheckTime;
        private ulong _totalSize;
        private float _waitTime;
        private WebClient _webClient;

        /// <inheritdoc />
        public override ulong DownloadedSize {
            get {
                lock (_lockObj) {
                    return _downloadedSize;
                }
            }
        }

        /// <inheritdoc />
        public override ulong TotalSize {
            get {
                lock (_lockObj) {
                    return _totalSize;
                }
            }
        }

        /// <inheritdoc />
        public override float Progress {
            get {
                lock (_lockObj) {
                    return _progress;
                }
            }
        }

        /// <summary>
        ///     Resets state, creates a <see cref="WebClient" />, and begins
        ///     an asynchronous file download from the configured task URL.
        /// </summary>
        protected override void OnStart() {
            _downloadedSize = 0;
            _totalSize = 0;
            _progress = 0f;
            _waitTime = 0f;
            _progressCheckTime = 0f;
            _done = false;
            _error = null;

            _webClient = new WebClient();
            _webClient.Headers.Add("user-agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)");
            _webClient.DownloadFileCompleted += OnDownloadFileCompleted;
            _webClient.DownloadProgressChanged += OnDownloadProgressChanged;

            try {
                _webClient.DownloadFileAsync(new Uri(Task.DownloadUrl), Task.DownloadPath);
            }
            catch (Exception e) {
                NotifyError((_error = e).ToString());
            }
        }

        /// <summary>
        ///     Cancels any in-progress download and disposes the <see cref="WebClient" />.
        /// </summary>
        protected override void OnStop() {
            if (_webClient != null) {
                _webClient.CancelAsync();
                _webClient.Dispose();
                _webClient = null;
            }
            _error = null;
        }

        /// <summary>
        ///     Checks for timeout, reports progress updates at the configured
        ///     interval, and notifies completion or error once the download finishes.
        /// </summary>
        /// <param name="elapseSeconds">Seconds elapsed since the last frame.</param>
        protected override void OnUpdate(float elapseSeconds) {
            if (_webClient == null) {
                return;
            }

            if (!_done) {
                _waitTime += elapseSeconds;
                if (Timeout > 0 && _waitTime >= Timeout) {
                    NotifyError("Download file Timeout.");
                    return;
                }

                _progressCheckTime += elapseSeconds;
                if (_progressCheckTime >= ProgressUpdateInterval) {
                    if (_lastDownloadedSize < _downloadedSize) {
                        _lastDownloadedSize = _downloadedSize;
                        _waitTime = 0f;
                        NotifyUpdate();
                    }

                    _progressCheckTime = 0f;
                }

                return;
            }

            lock (_lockObj) {
                if (_error != null) {
                    NotifyError(_error.ToString());
                }
                else {
                    NotifyComplete();
                }
            }
        }

        /// <summary>
        ///     Handles the <see cref="WebClient.DownloadProgressChanged" /> event,
        ///     updating byte counts and progress under the synchronization lock.
        /// </summary>
        /// <param name="sender">The event source.</param>
        /// <param name="e">Progress information provided by the WebClient.</param>
        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            lock (_lockObj) {
                _downloadedSize = (ulong)e.BytesReceived;
                _totalSize = (ulong)e.TotalBytesToReceive;
                _progress = (float)e.ProgressPercentage / 100;
            }
        }

        /// <summary>
        ///     Handles the <see cref="WebClient.DownloadFileCompleted" /> event,
        ///     capturing any error and signaling completion under the lock.
        /// </summary>
        /// <param name="sender">The event source.</param>
        /// <param name="e">Completion information provided by the WebClient.</param>
        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
            lock (_lockObj) {
                if (e.Error != null) {
                    _error = e.Error;
                }
                _done = true;
            }
        }
    }
}
