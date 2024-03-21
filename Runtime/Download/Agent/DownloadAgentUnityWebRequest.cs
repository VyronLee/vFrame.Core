using System;
using System.IO;
using UnityEngine.Networking;
using vFrame.Core.Exceptions;
using Logger = vFrame.Core.Loggers.Logger;

namespace vFrame.Core.Unity.Download
{
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

        public override ulong DownloadedSize => _downloadedSize;

        public override ulong TotalSize => _totalSize;

        public override float Progress => _progress;

        protected override void OnStart() {
            StopTask(true);

            _downloadedSize = 0ul;
            _totalSize = 0ul;
            _progress = 0f;
            _progressCheckTime = 0f;

            _state = DownloadProcessState.NotStart;
        }

        protected override void OnStop() {
            StopTask(true);
        }

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

        private void DownloadFileHead() {
            var uri = new Uri(Task.DownloadUrl);
            _headRequest = UnityWebRequest.Head(uri);
            _headRequest.SendWebRequest();
            _state = DownloadProcessState.HeadRequesting;
        }

        private void UpdateHeadDownloadProgress() {
            if (null == _headRequest) {
                return;
            }

            if (!_headRequest.isDone) {
                return;
            }

            if (IsWebRequestError(_headRequest)) {
                Logger.Warning("Request file head failed, uri: {0}, error: {1}", Task.DownloadUrl, _headRequest.error);
                _state = DownloadProcessState.HeadRequested;
                _totalSize = 1;
                return;
            }

            var size = _headRequest.GetResponseHeader("Content-Length");
            if (string.IsNullOrEmpty(size)) {
                Logger.Warning("Retrieve file size failed, uri: {0}", Task.DownloadUrl);
                _state = DownloadProcessState.HeadRequested;
                _totalSize = 1;
                return;
            }

            try {
                _totalSize = Convert.ToUInt32(size);
            }
            catch (Exception e) {
                Logger.Warning("Parse file size failed: {0}, exception: {1}", size, e);
                _state = DownloadProcessState.HeadRequested;
                _totalSize = 1;
                return;
            }
            _state = DownloadProcessState.HeadRequested;

            //Logger.Info("File head downloaded, size: {0}, url: {1}", size, Task.DownloadUrl);
        }

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
                Logger.Warning("Download error, uri: {0}, msg: {1}", Task.DownloadUrl, _contentRequest.error);
                _state = DownloadProcessState.Error;
                _error = _contentRequest.error;
                return;
            }

            _state = DownloadProcessState.ContentDownloaded;
        }

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