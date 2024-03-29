using System;
using System.Collections.Generic;
using UnityEngine;
using vFrame.Core.Unity.Extensions;

namespace vFrame.Core.Unity.Download
{
    public class Downloader : MonoBehaviour
    {
        private const int DownloadAgentCount = 3;

        [SerializeField]
        private int _timeout = 300;

        [SerializeField]
        private float _progressUpdateInterval = 0.1f;

        private readonly List<IDownloadAgent> _agents = new List<IDownloadAgent>();
        private readonly DownloadSpeedCounter _speedCounter = new DownloadSpeedCounter();
        private readonly LinkedList<DownloadTask> _waitingTasks = new LinkedList<DownloadTask>();

        public int Timeout {
            get => _timeout;
            set {
                if (value <= 0) {
                    return;
                }
                _timeout = value;
                foreach (var agent in _agents) {
                    agent.Timeout = value;
                }
            }
        }

        public float ProgressUpdateInterval {
            get => _progressUpdateInterval;
            set {
                _progressUpdateInterval = value;
                foreach (var agent in _agents) {
                    agent.ProgressUpdateInterval = value;
                }
            }
        }

        public float Speed {
            get {
                if (!enabled) {
                    return 0f;
                }
                return _speedCounter.Speed;
            }
        }

        public string FormattedSpeed => FormatSpeed(Speed);

        public bool IsPaused { get; set; }

        private void Awake() {
            for (var i = 0; i < DownloadAgentCount; i++) {
                AddDownloadAgent(new DownloadAgentUnityWebRequest());
            }
        }

        private void Update() {
            UpdateDownloadSpeed();
            UpdateDownloadAgent();
        }

        public event Action<DownloadEventArgs> DownloadStart;
        public event Action<DownloadEventArgs> DownloadUpdate;
        public event Action<DownloadEventArgs> DownloadSuccess;
        public event Action<DownloadEventArgs> DownloadFailure;

        public static Downloader Create(string name = "DownloadManager") {
            var go = new GameObject(name).DontDestroyEx().DontSaveAndHideEx();
            var inst = go.AddComponent<Downloader>();
            return inst;
        }

        private void AddDownloadAgent(IDownloadAgent agent) {
            agent.Timeout = _timeout;
            agent.ProgressUpdateInterval = _progressUpdateInterval;
            agent.DownloadAgentStart += OnDownloadAgentStart;
            agent.DownloadAgentUpdate += OnDownloadAgentUpdate;
            agent.DownloadAgentSuccess += OnDownloadAgentSuccess;
            agent.DownloadAgentFailure += OnDownloadAgentFailure;

            _agents.Add(agent);
        }

        public DownloadTask AddDownload(string downloadPath, string downloadUrl, object userData = null) {
            enabled = true;

            var downloadTask = new DownloadTask(downloadPath, downloadUrl, userData);
            _waitingTasks.AddLast(downloadTask);

            return downloadTask;
        }

        public void RemoveDownload(int taskId) {
            foreach (var task in _waitingTasks) {
                if (task.TaskId == taskId) {
                    _waitingTasks.Remove(task);
                    return;
                }
            }

            foreach (var agent in _agents) {
                if (agent.Task != null && agent.Task.TaskId == taskId) {
                    agent.Stop();
                    return;
                }
            }
        }

        public void RemoveAllDownloads() {
            _waitingTasks.Clear();

            foreach (var agent in _agents) {
                agent.Stop();
            }

            enabled = false;
        }

        public DownloadTask GetDownload(int serialId) {
            foreach (var task in _waitingTasks) {
                if (task.TaskId == serialId) {
                    return task;
                }
            }

            foreach (var agent in _agents) {
                if (agent.Task != null && agent.Task.TaskId == serialId) {
                    return agent.Task;
                }
            }

            return null;
        }

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

        private void UpdateDownloadSpeed() {
            for (var i = 0; i < _agents.Count; i++) {
                _speedCounter.AddDownloadSize(_agents[i].DownloadedSizeDelta);
            }
            _speedCounter.Update(Time.unscaledDeltaTime);
        }

        private void UpdateDownloadAgent() {
            var hasRunningTask = false;
            for (var i = 0; i < _agents.Count; i++) {
                var agent = _agents[i];
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
            if (_waitingTasks.Count > 0) {
                agent.Start(_waitingTasks.First.Value);
                _waitingTasks.RemoveFirst();
                return true;
            }
            return false;
        }

        private void OnDownloadAgentStart(IDownloadAgent sender) {
            var args = new DownloadEventArgs {
                SerialId = sender.Task.TaskId,
                UserData = sender.Task.UserData
            };

            if (DownloadStart != null) {
                DownloadStart(args);
            }

            sender.Task.NotifyStart(args);
        }

        private void OnDownloadAgentUpdate(IDownloadAgent sender) {
            var args = new DownloadEventArgs {
                SerialId = sender.Task.TaskId,
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
                SerialId = sender.Task.TaskId,
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
                SerialId = sender.Task.TaskId,
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