using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using vFrame.Core.Singletons;

namespace vFrame.Core.Download
{
    public class DownloadManager : MonoSingleton<DownloadManager>
    {
        private const int DOWNLOAD_AGENT_COUNT = 3;

        [SerializeField] private float m_Timeout = 30f;

        [SerializeField] private float m_ProgressUpdateInterval = 0.1f;

        private readonly List<IDownloadAgent> m_Agents = new List<IDownloadAgent>();
        private readonly LinkedList<DownloadTask> m_WaitingTasks = new LinkedList<DownloadTask>();

        private float m_CachedSpeed;
        private float m_LastGetSpeedTime;
        private const float GET_SPEED_INTERVAL = 1f;

        public event Action<DownloadEventArgs> DownloadStart;
        public event Action<DownloadEventArgs> DownloadUpdate;
        public event Action<DownloadEventArgs> DownloadSuccess;
        public event Action<DownloadEventArgs> DownloadFailure;

        public float Timeout {
            get { return m_Timeout; }
            set {
                m_Timeout = value;
                foreach (var agent in m_Agents) {
                    agent.Timeout = value;
                }
            }
        }

        public float ProgressUpdateInterval {
            get { return m_ProgressUpdateInterval; }
            set {
                m_ProgressUpdateInterval = value;
                foreach (var agent in m_Agents) {
                    agent.ProgressUpdateInterval = value;
                }
            }
        }

        public float Speed {
            get {
                if (!enabled) {
                    return 0f;
                }

                var curTime = Time.realtimeSinceStartup;
                if (curTime - m_LastGetSpeedTime >= GET_SPEED_INTERVAL) {
                    m_CachedSpeed = 0f;
                    foreach (var agent in m_Agents) {
                        m_CachedSpeed += agent.Speed;
                    }

                    m_LastGetSpeedTime = curTime;
                }

                return m_CachedSpeed;
            }
        }

        public string FormatedSpeed {
            get { return FormatSpeed(Speed); }
        }

        public float GetDownloadSpeed(int serialId) {
            if (!enabled) {
                return 0f;
            }

            foreach (var agent in m_Agents) {
                if (agent.Task != null && agent.Task.SerialId == serialId) {
                    return agent.Speed;
                }
            }

            return 0f;
        }

        public string GetFormatedDownloadSpeed(int serialId) {
            return FormatSpeed(GetDownloadSpeed(serialId));
        }

        protected new void Awake() {
            base.Awake();

            for (var i = 0; i < DOWNLOAD_AGENT_COUNT; i++) {
                AddDownloadAgent(new DownloadAgentWebClient());
            }

            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
        }

        private void AddDownloadAgent(IDownloadAgent agent) {
            agent.Timeout = m_Timeout;
            agent.ProgressUpdateInterval = m_ProgressUpdateInterval;
            agent.DownloadAgentStart += OnDownloadAgentStart;
            agent.DownloadAgentUpdate += OnDownloadAgentUpdate;
            agent.DownloadAgentSuccess += OnDownloadAgentSuccess;
            agent.DownloadAgentFailure += OnDownloadAgentFailure;

            m_Agents.Add(agent);
        }

        public DownloadTask AddDownload(string downloadPath, string downloadUrl, object userData = null) {
            enabled = true;

            var downloadTask = new DownloadTask(downloadPath, downloadUrl, userData);
            m_WaitingTasks.AddLast(downloadTask);

            return downloadTask;
        }

        public void RemoveDownload(int serialId) {
            foreach (var task in m_WaitingTasks) {
                if (task.SerialId == serialId) {
                    m_WaitingTasks.Remove(task);
                    return;
                }
            }

            foreach (var agent in m_Agents) {
                if (agent.Task != null && agent.Task.SerialId == serialId) {
                    agent.Stop();
                    return;
                }
            }
        }

        public void RemoveAllDownloads() {
            m_WaitingTasks.Clear();

            foreach (var agent in m_Agents) {
                agent.Stop();
            }

            enabled = false;
        }

        private void Update() {
            var hasRunningTask = false;
            for (var i = 0; i < m_Agents.Count; i++) {
                var agent = m_Agents[i];
                if (agent.Task != null) {
                    if (agent.TaskDone) {
                        agent.Stop();
                        hasRunningTask |= StartAnotherTask(agent);
                    }
                    else {
                        agent.Update(Time.unscaledDeltaTime);
                        hasRunningTask = true;
                    }
                }
                else {
                    hasRunningTask |= StartAnotherTask(agent);
                }
            }

            if (!hasRunningTask) {
                enabled = false;
            }
        }

        private bool StartAnotherTask(IDownloadAgent agent) {
            if (m_WaitingTasks.Count > 0) {
                agent.Start(m_WaitingTasks.First.Value);
                m_WaitingTasks.RemoveFirst();
                return true;
            }

            return false;
        }

        private void OnDownloadAgentStart(IDownloadAgent sender) {
            var args = new DownloadEventArgs {
                SerialId = sender.Task.SerialId,
                UserData = sender.Task.UserData
            };

            if (DownloadStart != null) {
                DownloadStart(args);
            }

            sender.Task.NotifyStart(args);
        }

        private void OnDownloadAgentUpdate(IDownloadAgent sender) {
            var args = new DownloadEventArgs {
                SerialId = sender.Task.SerialId,
                UserData = sender.Task.UserData,
                DownloadedSize = sender.DownloadedSize,
                TotalSize = sender.TotalSize,
                Speed = sender.Speed,
                Progress = sender.Progress
            };

            if (DownloadUpdate != null) {
                DownloadUpdate(args);
            }

            sender.Task.NotifyUpdate(args);
        }

        private void OnDownloadAgentSuccess(IDownloadAgent sender) {
            var args = new DownloadEventArgs {
                SerialId = sender.Task.SerialId,
                UserData = sender.Task.UserData,
                DownloadedSize = sender.DownloadedSize,
                TotalSize = sender.TotalSize,
                Speed = sender.Speed,
                Progress = 1
            };

            if (DownloadSuccess != null) {
                DownloadSuccess(args);
            }

            sender.Task.NotifySuccess(args);
        }

        private void OnDownloadAgentFailure(IDownloadAgent sender, string errorMessage) {
            var args = new DownloadEventArgs {
                SerialId = sender.Task.SerialId,
                UserData = sender.Task.UserData,
                Error = errorMessage
            };

            if (DownloadFailure != null) {
                DownloadFailure(args);
            }

            sender.Task.NotifyFailure(args);
        }

        public static string FormatSpeed(float speed) {
            // less than 1MB/s
            if (speed < 1024 * 1024) {
                return (speed / 1024).ToString("#0.0") + "KB/s";
            }

            return (speed / (1024 * 1024)).ToString("#0.0") + "MB/s";
        }
    }
}