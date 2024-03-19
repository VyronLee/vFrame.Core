using System;
using System.ComponentModel;
using System.Net;

namespace vFrame.Core.Unity.Download
{
    public class DownloadAgentWebClient : DownloadAgentBase
    {
        private readonly object m_LockObj = new object();
        private bool m_Done;
        private ulong m_DownloadedSize;
        private Exception m_Error;

        private ulong m_LastDownloadedSize;
        private float m_Progress;
        private float m_ProgressCheckTime;
        private ulong m_TotalSize;
        private float m_WaitTime;
        private WebClient m_WebClient;

        public override ulong DownloadedSize {
            get {
                lock (m_LockObj) {
                    return m_DownloadedSize;
                }
            }
        }

        public override ulong TotalSize {
            get {
                lock (m_LockObj) {
                    return m_TotalSize;
                }
            }
        }

        public override float Progress {
            get {
                lock (m_LockObj) {
                    return m_Progress;
                }
            }
        }

        protected override void OnStart() {
            m_DownloadedSize = 0;
            m_TotalSize = 0;
            m_Progress = 0f;
            m_WaitTime = 0f;
            m_ProgressCheckTime = 0f;
            m_Done = false;
            m_Error = null;

            m_WebClient = new WebClient();
            m_WebClient.Headers.Add("user-agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)");
            m_WebClient.DownloadFileCompleted += OnDownloadFileCompleted;
            m_WebClient.DownloadProgressChanged += OnDownloadProgressChanged;

            try {
                m_WebClient.DownloadFileAsync(new Uri(Task.DownloadUrl), Task.DownloadPath);
            }
            catch (Exception e) {
                NotifyError((m_Error = e).ToString());
            }
        }

        protected override void OnStop() {
            if (m_WebClient != null) {
                m_WebClient.CancelAsync();
                m_WebClient.Dispose();
                m_WebClient = null;
            }
            m_Error = null;
        }

        protected override void OnUpdate(float elapseSeconds) {
            if (m_WebClient == null) {
                return;
            }

            if (!m_Done) {
                m_WaitTime += elapseSeconds;
                if (Timeout > 0 && m_WaitTime >= Timeout) {
                    NotifyError("Download file Timeout.");
                    return;
                }

                m_ProgressCheckTime += elapseSeconds;
                if (m_ProgressCheckTime >= ProgressUpdateInterval) {
                    if (m_LastDownloadedSize < m_DownloadedSize) {
                        m_LastDownloadedSize = m_DownloadedSize;
                        m_WaitTime = 0f;
                        NotifyUpdate();
                    }

                    m_ProgressCheckTime = 0f;
                }

                return;
            }

            lock (m_LockObj) {
                if (m_Error != null) {
                    NotifyError(m_Error.ToString());
                }
                else {
                    NotifyComplete();
                }
            }
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            lock (m_LockObj) {
                m_DownloadedSize = (ulong)e.BytesReceived;
                m_TotalSize = (ulong)e.TotalBytesToReceive;
                m_Progress = (float)e.ProgressPercentage / 100;
            }
        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
            lock (m_LockObj) {
                if (e.Error != null) {
                    m_Error = e.Error;
                }
                m_Done = true;
            }
        }
    }
}