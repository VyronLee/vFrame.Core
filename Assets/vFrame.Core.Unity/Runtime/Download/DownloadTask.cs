using System;

namespace vFrame.Core.Unity.Download
{
    public sealed class DownloadTask
    {
        private static int s_Serial;

        public DownloadTask(string downloadPath, string downloadUrl, object userData) {
            SerialId = s_Serial++;
            DownloadPath = downloadPath;
            DownloadUrl = downloadUrl;
            UserData = userData;
        }

        public int SerialId { get; }

        public string DownloadPath { get; }

        public string DownloadUrl { get; }

        public object UserData { get; }

        public event Action<DownloadEventArgs> DownloadStart;
        public event Action<DownloadEventArgs> DownloadUpdate;
        public event Action<DownloadEventArgs> DownloadSuccess;
        public event Action<DownloadEventArgs> DownloadFailure;

        public void NotifyStart(DownloadEventArgs args) {
            if (DownloadStart != null) {
                DownloadStart(args);
            }
        }

        public void NotifyUpdate(DownloadEventArgs args) {
            if (DownloadUpdate != null) {
                DownloadUpdate(args);
            }
        }

        public void NotifySuccess(DownloadEventArgs args) {
            if (DownloadSuccess != null) {
                DownloadSuccess(args);
            }
        }

        public void NotifyFailure(DownloadEventArgs args) {
            if (DownloadFailure != null) {
                DownloadFailure(args);
            }
        }
    }
}