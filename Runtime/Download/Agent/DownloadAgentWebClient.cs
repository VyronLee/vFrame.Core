using System;
using System.ComponentModel;
using System.Net;

namespace vFrame.Core.Unity.Download
{
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

        public override ulong DownloadedSize {
            get {
                lock (_lockObj) {
                    return _downloadedSize;
                }
            }
        }

        public override ulong TotalSize {
            get {
                lock (_lockObj) {
                    return _totalSize;
                }
            }
        }

        public override float Progress {
            get {
                lock (_lockObj) {
                    return _progress;
                }
            }
        }

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

        protected override void OnStop() {
            if (_webClient != null) {
                _webClient.CancelAsync();
                _webClient.Dispose();
                _webClient = null;
            }
            _error = null;
        }

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

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            lock (_lockObj) {
                _downloadedSize = (ulong)e.BytesReceived;
                _totalSize = (ulong)e.TotalBytesToReceive;
                _progress = (float)e.ProgressPercentage / 100;
            }
        }

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