// ------------------------------------------------------------
//         File: DownloadAgentUnityWebRequest.cs
//        Brief: Download agent implementation that uses
//                UnityWebRequest to perform HEAD and GET requests
//                for file size detection and content downloading.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.IO;
using UnityEngine.Networking;

namespace vFrame.Core.Unity
{
    /// <summary>
    ///     <see cref="DownloadAgentBase" /> implementation backed by
    ///     <see cref="UnityWebRequest" /> for HTTP file downloads.
    ///     Performs a HEAD request first to retrieve content length, then
    ///     streams the body to disk via <see cref="DownloadHandlerFile" />.
    /// </summary>
    public class DownloadAgentUnityWebRequest : DownloadAgentBase
    {
        private UnityWebRequest _contentRequest;
        private DownloadHandlerFile _downloadHandler;
        private UnityWebRequest _headRequest;
        private DownloadProcessState _state;

        private string _error;
        private ulong _downloadedSize;
        private float _progress;
        private float _progressCheckTime;
        private ulong _totalSize;

        /// <inheritdoc />
        public override ulong DownloadedSize => _downloadedSize;

        /// <inheritdoc />
        public override ulong TotalSize => _totalSize;

        /// <inheritdoc />
        public override float Progress => _progress;

        /// <summary>
        ///     Resets all internal state and aborts any in-flight requests.
        /// </summary>
        protected override void OnStart() {
            StopTask(true);

            _downloadedSize = 0ul;
            _totalSize = 0ul;
            _progress = 0f;
            _progressCheckTime = 0f;

            _state = DownloadProcessState.NotStart;
        }

        /// <summary>
        ///     Aborts and disposes all active web requests.
        /// </summary>
        protected override void OnStop() {
            StopTask(true);
        }

        /// <summary>
        ///     Aborts and disposes the HEAD request, content request,
        ///     and download handler, optionally aborting the content request.
        /// </summary>
        /// <param name="abort">
        ///     <c>true</c> to abort the content request before disposing;
        ///     <c>false</c> to dispose without aborting (used on successful completion).
        /// </param>
        private void StopTask(bool abort) {
            if (_headRequest != null) {
                _headRequest.Abort();
                _headRequest.Dispose();
                _headRequest = null;
            }

            if (_contentRequest != null) {
                if (abort) {
                    _contentRequest.Abort();
                }
                _contentRequest.Dispose();
                _contentRequest = null;
            }

            if (_downloadHandler != null) {
                _downloadHandler.Dispose();
                _downloadHandler = null;
            }
        }

        /// <summary>
        ///     Drives the download state machine forward each frame.
        /// </summary>
        /// <param name="elapseSeconds">Seconds elapsed since the last frame.</param>
        protected override void OnUpdate(float elapseSeconds) {
            switch (_state) {
                case DownloadProcessState.NotStart:
                    DownloadFileHead();
                    break;
                case DownloadProcessState.HeadRequesting:
                    UpdateHeadDownloadProgress();
                    break;
                case DownloadProcessState.HeadRequested:
                    DownloadFileContent();
                    break;
                case DownloadProcessState.ContentDownloading:
                    UpdateDownloadProgress(elapseSeconds);
                    break;
                case DownloadProcessState.ContentDownloaded:
                    StopTask(false);
                    NotifyComplete();
                    break;
                case DownloadProcessState.Error:
                    StopTask(true);
                    NotifyError(_error);
                    break;
                default:
                    ThrowHelper.ThrowUnsupportedEnum(_state);
                    break;
            }
        }

        /// <summary>
        ///     Sends a HEAD request to determine the remote file size.
        /// </summary>
        private void DownloadFileHead() {
            var uri = new Uri(Task.DownloadUrl);
            _headRequest = UnityWebRequest.Head(uri);
            _headRequest.SendWebRequest();
            _state = DownloadProcessState.HeadRequesting;
        }

        /// <summary>
        ///     Checks whether the HEAD request has completed and, on success,
        ///     parses the <c>Content-Length</c> header into <see cref="_totalSize" />.
        ///     Falls back to a size of 1 when the header is unavailable.
        /// </summary>
        private void UpdateHeadDownloadProgress() {
            if (null == _headRequest) {
                return;
            }

            if (!_headRequest.isDone) {
                return;
            }

            if (IsWebRequestError(_headRequest)) {
                Logger.Warning($"Request file head failed, uri: {Task.DownloadUrl}, error: {_headRequest.error}");
                _state = DownloadProcessState.HeadRequested;
                _totalSize = 1;
                return;
            }

            var size = _headRequest.GetResponseHeader("Content-Length");
            if (string.IsNullOrEmpty(size)) {
                Logger.Warning($"Retrieve file size failed, uri: {Task.DownloadUrl}");
                _state = DownloadProcessState.HeadRequested;
                _totalSize = 1;
                return;
            }

            try {
                _totalSize = Convert.ToUInt32(size);
            }
            catch (Exception e) {
                Logger.Warning($"Parse file size failed: {size}, exception: {e}");
                _state = DownloadProcessState.HeadRequested;
                _totalSize = 1;
                return;
            }
            _state = DownloadProcessState.HeadRequested;
        }

        /// <summary>
        ///     Starts the content download by creating a
        ///     <see cref="DownloadHandlerFile" /> targeting
        ///     <see cref="DownloadTask.DownloadPath" />.
        ///     Any existing file at the target path is deleted first.
        /// </summary>
        private void DownloadFileContent() {
            if (File.Exists(Task.DownloadPath)) {
                File.Delete(Task.DownloadPath);
            }
            _downloadHandler = new DownloadHandlerFile(Task.DownloadPath);
            _downloadHandler.removeFileOnAbort = true;

            var uri = new Uri(Task.DownloadUrl);
            _contentRequest = UnityWebRequest.Get(uri);
            _contentRequest.downloadHandler = _downloadHandler;
            _contentRequest.SendWebRequest();
            _state = DownloadProcessState.ContentDownloading;
        }

        /// <summary>
        ///     Polls the content request for completion and updates
        ///     <see cref="_downloadedSize" /> and <see cref="_progress" />
        ///     at the configured <see cref="DownloadAgentBase.ProgressUpdateInterval" />.
        /// </summary>
        /// <param name="elapsedTime">Seconds elapsed since the last frame.</param>
        private void UpdateDownloadProgress(float elapsedTime) {
            if (null == _contentRequest) {
                return;
            }

            if (!_contentRequest.isDone) {
                _progressCheckTime += elapsedTime;
                if (_progressCheckTime >= ProgressUpdateInterval) {
                    _progressCheckTime = 0f;
                    _downloadedSize = _contentRequest.downloadedBytes;
                    _progress = _contentRequest.downloadProgress;
                }
                NotifyUpdate();
                return;
            }

            if (IsWebRequestError(_contentRequest)) {
                Logger.Warning($"Download error, uri: {Task.DownloadUrl}, msg: {_contentRequest.error}");
                _state = DownloadProcessState.Error;
                _error = _contentRequest.error;
                return;
            }

            _state = DownloadProcessState.ContentDownloaded;
        }

        /// <summary>
        ///     Determines whether a <see cref="UnityWebRequest" /> finished
        ///     with a connection or protocol error, accounting for API
        ///     differences across Unity versions.
        /// </summary>
        /// <param name="request">The request to inspect.</param>
        /// <returns><c>true</c> if the request encountered an error; otherwise <c>false</c>.</returns>
        private bool IsWebRequestError(UnityWebRequest request) {
#if UNITY_2020_1_OR_NEWER
            return request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError;
#else
            return request.isHttpError || request.isNetworkError;
#endif
        }

        private enum DownloadProcessState
        {
            NotStart,
            HeadRequesting,
            HeadRequested,
            ContentDownloading,
            ContentDownloaded,
            Error
        }
    }
}