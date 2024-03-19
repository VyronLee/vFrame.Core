using System;
using System.IO;
using UnityEngine.Networking;
using vFrame.Core.Exceptions;
using Logger = vFrame.Core.Loggers.Logger;

namespace vFrame.Core.Unity.Download
{
    public class DownloadAgentUnityWebRequest : DownloadAgentBase
    {
        private UnityWebRequest m_ContentRequest;
        private ulong m_DownloadedSize;
        private DownloadHandlerFile m_DownloadHandler;
        private string m_Error;
        private UnityWebRequest m_HeadRequest;
        private float m_Progress;
        private float m_ProgressCheckTime;
        private DownloadProcessState m_State;
        private ulong m_TotalSize;

        public override ulong DownloadedSize => m_DownloadedSize;

        public override ulong TotalSize => m_TotalSize;

        public override float Progress => m_Progress;

        protected override void OnStart() {
            StopTask(true);

            m_DownloadedSize = 0ul;
            m_TotalSize = 0ul;
            m_Progress = 0f;
            m_ProgressCheckTime = 0f;

            m_State = DownloadProcessState.NotStart;
        }

        protected override void OnStop() {
            StopTask(true);
        }

        private void StopTask(bool abort) {
            if (m_HeadRequest != null) {
                m_HeadRequest.Abort();
                m_HeadRequest.Dispose();
                m_HeadRequest = null;
            }

            if (m_ContentRequest != null) {
                if (abort) {
                    m_ContentRequest.Abort();
                }
                m_ContentRequest.Dispose();
                m_ContentRequest = null;
            }

            if (m_DownloadHandler != null) {
                m_DownloadHandler.Dispose();
                m_DownloadHandler = null;
            }
        }

        protected override void OnUpdate(float elapseSeconds) {
            switch (m_State) {
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
                    NotifyError(m_Error);
                    break;
                default:
                    ThrowHelper.ThrowUnsupportedEnum(m_State);
                    break;
            }
        }

        private void DownloadFileHead() {
            var uri = new Uri(Task.DownloadUrl);
            m_HeadRequest = UnityWebRequest.Head(uri);
            m_HeadRequest.SendWebRequest();
            m_State = DownloadProcessState.HeadRequesting;
        }

        private void UpdateHeadDownloadProgress() {
            if (null == m_HeadRequest) {
                return;
            }

            if (!m_HeadRequest.isDone) {
                return;
            }

            if (IsWebRequestError(m_HeadRequest)) {
                Logger.Warning("Request file head failed, uri: {0}, error: {1}", Task.DownloadUrl, m_HeadRequest.error);
                m_State = DownloadProcessState.HeadRequested;
                m_TotalSize = 1;
                return;
            }

            var size = m_HeadRequest.GetResponseHeader("Content-Length");
            if (string.IsNullOrEmpty(size)) {
                Logger.Warning("Retrieve file size failed, uri: {0}", Task.DownloadUrl);
                m_State = DownloadProcessState.HeadRequested;
                m_TotalSize = 1;
                return;
            }

            try {
                m_TotalSize = Convert.ToUInt32(size);
            }
            catch (Exception e) {
                Logger.Warning("Parse file size failed: {0}, exception: {1}", size, e);
                m_State = DownloadProcessState.HeadRequested;
                m_TotalSize = 1;
                return;
            }
            m_State = DownloadProcessState.HeadRequested;

            //Logger.Info("File head downloaded, size: {0}, url: {1}", size, Task.DownloadUrl);
        }

        private void DownloadFileContent() {
            if (File.Exists(Task.DownloadPath)) {
                File.Delete(Task.DownloadPath);
            }
            m_DownloadHandler = new DownloadHandlerFile(Task.DownloadPath);
            m_DownloadHandler.removeFileOnAbort = true;

            var uri = new Uri(Task.DownloadUrl);
            m_ContentRequest = UnityWebRequest.Get(uri);
            m_ContentRequest.downloadHandler = m_DownloadHandler;
            m_ContentRequest.SendWebRequest();
            m_State = DownloadProcessState.ContentDownloading;
        }

        private void UpdateDownloadProgress(float elapsedTime) {
            if (null == m_ContentRequest) {
                return;
            }

            if (!m_ContentRequest.isDone) {
                m_ProgressCheckTime += elapsedTime;
                if (m_ProgressCheckTime >= ProgressUpdateInterval) {
                    m_ProgressCheckTime = 0f;
                    m_DownloadedSize = m_ContentRequest.downloadedBytes;
                    m_Progress = m_ContentRequest.downloadProgress;
                }
                NotifyUpdate();
                return;
            }

            if (IsWebRequestError(m_ContentRequest)) {
                Logger.Warning("Download error, uri: {0}, msg: {1}", Task.DownloadUrl, m_ContentRequest.error);
                m_State = DownloadProcessState.Error;
                m_Error = m_ContentRequest.error;
                return;
            }

            m_State = DownloadProcessState.ContentDownloaded;
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