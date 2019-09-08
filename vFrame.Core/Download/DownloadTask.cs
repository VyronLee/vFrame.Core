﻿using System;

namespace vFrame.Core
{
    public sealed class DownloadTask
    {
        private static int s_Serial = 0;

        private readonly int m_SerialId;
        private readonly string m_DownloadPath;
        private readonly string m_DownloadUrl;
        private readonly object m_UserData;
        
        public event Action<DownloadEventArgs> DownloadStart;
        public event Action<DownloadEventArgs> DownloadUpdate;
        public event Action<DownloadEventArgs> DownloadSuccess;
        public event Action<DownloadEventArgs> DownloadFailure;

        public DownloadTask(string downloadPath, string downloadUrl, object userData)
        {
            m_SerialId = s_Serial++;
            m_DownloadPath = downloadPath;
            m_DownloadUrl = downloadUrl;
            m_UserData = userData;
        }

        public int SerialId
        {
            get { return m_SerialId; }
        }

        public string DownloadPath
        {
            get { return m_DownloadPath; }
        }

        public string DownloadUrl
        {
            get { return m_DownloadUrl; }
        }

        public object UserData
        {
            get { return m_UserData; }
        }

        public void NotifyStart(DownloadEventArgs args)
        {
            if (DownloadStart != null)
            {
                DownloadStart(args);
            }
        }
        
        public void NotifyUpdate(DownloadEventArgs args)
        {
            if (DownloadUpdate != null)
            {
                DownloadUpdate(args);
            }
        }

        public void NotifySuccess(DownloadEventArgs args)
        {
            if (DownloadSuccess != null)
            {
                DownloadSuccess(args);
            }
        }

        public void NotifyFailure(DownloadEventArgs args)
        {
            if (DownloadFailure != null)
            {
                DownloadFailure(args);
            }
        }
    }
}