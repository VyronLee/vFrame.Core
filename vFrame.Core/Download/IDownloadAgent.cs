﻿using System;

namespace vFrame.Core
{
    public interface IDownloadAgent
    {
        DownloadTask Task { get; }
        
        bool TaskDone { get; }
        
        ulong DownloadedSize { get; }
        
        ulong TotalSize { get; }
        
        float Progress { get; }
        
        float Speed { get; }
        
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