using System;

namespace vFrame.Core.Download
{
    public interface IDownloadAgent
    {
        DownloadTask Task { get; }

        bool TaskDone { get; }

        ulong DownloadedSizeDelta { get; }

        ulong DownloadedSize { get; }

        ulong TotalSize { get; }

        float Progress { get; }

        float Timeout { get; set; }

        float ProgressUpdateInterval { get; set; }

        event Action<IDownloadAgent> DownloadAgentStart;
        event Action<IDownloadAgent> DownloadAgentUpdate;
        event Action<IDownloadAgent> DownloadAgentSuccess;
        event Action<IDownloadAgent, string> DownloadAgentFailure;

        void Start(DownloadTask task);

        void Stop();

        void Update(float elapseSeconds);
    }
}