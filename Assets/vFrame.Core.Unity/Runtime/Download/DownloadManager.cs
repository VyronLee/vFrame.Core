using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace vFrame.Core.Unity.Download
{
    public class DownloadManager : MonoBehaviour
    {
        private const int DOWNLOAD_AGENT_COUNT = 3;

        [SerializeField] private int m_Timeout = 300;

        [SerializeField] private float m_ProgressUpdateInterval = 0.1f;

        private readonly List<IDownloadAgent> m_Agents = new List<IDownloadAgent>();
        private readonly LinkedList<DownloadTask> m_WaitingTasks = new LinkedList<DownloadTask>();
        private readonly DownloadSpeedCounter m_SpeedCounter = new DownloadSpeedCounter();

        public event Action<DownloadEventArgs> DownloadStart;
        public event Action<DownloadEventArgs> DownloadUpdate;
        public event Action<DownloadEventArgs> DownloadSuccess;
        public event Action<DownloadEventArgs> DownloadFailure;

        public int Timeout {
            get { return m_Timeout; }
            set {
                if (value <= 0) {
                    return;
                }
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
                return m_SpeedCounter.Speed;
            }
        }

        public string FormattedSpeed {
            get { return FormatSpeed(Speed); }
        }

        public static DownloadManager Create(string name = "DownloadManager") {
            var go = new GameObject(name) {hideFlags = HideFlags.HideAndDontSave};
            var inst = go.AddComponent<DownloadManager>();
            DontDestroyOnLoad(go);
            return inst;
        }

        private void Awake() {
            for (var i = 0; i < DOWNLOAD_AGENT_COUNT; i++) {
                AddDownloadAgent(new DownloadAgentUnityWebRequest());
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

        public DownloadTask GetDownload(int serialId) {
            foreach (var task in m_WaitingTasks) {
                if (task.SerialId == serialId) {
                    return task;
                }
            }

            foreach (var agent in m_Agents) {
                if (agent.Task != null && agent.Task.SerialId == serialId) {
                    return agent.Task;
                }
            }

            return null;
        }

        public bool IsPaused { get; set; }

        public void Pause() {
            if (IsPaused) {
                return;
            }
            IsPaused = true;
            enabled = false;
        }

        public void Resume() {
            if (!IsPaused) {
                return;
            }
            IsPaused = false;
            enabled = true;
        }

        private void Update() {
            UpdateDownloadSpeed();
            UpdateDownloadAgent();
        }

        private void UpdateDownloadSpeed() {
            for (var i = 0; i < m_Agents.Count; i++) {
                m_SpeedCounter.AddDownloadSize(m_Agents[i].DownloadedSizeDelta);
            }
            m_SpeedCounter.Update(Time.unscaledDeltaTime);
        }

        private void UpdateDownloadAgent() {
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