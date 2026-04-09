// ------------------------------------------------------------
//         File: Downloader.cs
//        Brief: MonoBehaviour-based download manager that schedules
//                tasks across multiple download agents and tracks
//                aggregate download speed.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-08 22:00:00
//    Copyright: Copyright (c) 2019, VyronLee
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace vFrame.Core.Unity
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

        /// <summary>
        /// Gets or sets the network timeout in seconds. Values less than or
        /// equal to zero are ignored. Changing the value propagates to all
        /// registered download agents.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the interval in seconds between progress updates.
        /// Changing the value propagates to all registered download agents.
        /// </summary>
        public float ProgressUpdateInterval {
            get => _progressUpdateInterval;
            set {
                _progressUpdateInterval = value;
                foreach (var agent in _agents) {
                    agent.ProgressUpdateInterval = value;
                }
            }
        }

        /// <summary>
        /// Gets the current aggregate download speed in bytes per second.
        /// Returns zero when this component is disabled.
        /// </summary>
        public float Speed {
            get {
                if (!enabled) {
                    return 0f;
                }
                return _speedCounter.Speed;
            }
        }

        /// <summary>
        /// Gets the current download speed formatted as a human-readable
        /// string (e.g. "1024.0KB/s" or "1.5MB/s").
        /// </summary>
        public string FormattedSpeed => FormatSpeed(Speed);

        /// <summary>
        /// Gets or sets whether all downloads are paused.
        /// </summary>
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

        /// <summary>
        /// Raised when a download task starts.
        /// </summary>
        public event Action<DownloadEventArgs> DownloadStart;

        /// <summary>
        /// Raised when a download task reports progress.
        /// </summary>
        public event Action<DownloadEventArgs> DownloadUpdate;

        /// <summary>
        /// Raised when a download task completes successfully.
        /// </summary>
        public event Action<DownloadEventArgs> DownloadSuccess;

        /// <summary>
        /// Raised when a download task fails.
        /// </summary>
        public event Action<DownloadEventArgs> DownloadFailure;

        /// <summary>
        /// Creates a new <see cref="Downloader"/> instance attached to a
        /// freshly created <see cref="GameObject"/> that persists across
        /// scene loads.
        /// </summary>
        /// <param name="name">Name of the created GameObject.</param>
        /// <returns>The created Downloader component.</returns>
        public static Downloader Create(string name = "DownloadManager") {
            var go = new GameObject(name).DontDestroyEx().DontSaveAndHideEx();
            var inst = go.AddComponent<Downloader>();
            return inst;
        }

        /// <summary>
        /// Registers a download agent and subscribes to its lifecycle events.
        /// </summary>
        /// <param name="agent">The agent to register.</param>
        private void AddDownloadAgent(IDownloadAgent agent) {
            agent.Timeout = _timeout;
            agent.ProgressUpdateInterval = _progressUpdateInterval;
            agent.DownloadAgentStart += OnDownloadAgentStart;
            agent.DownloadAgentUpdate += OnDownloadAgentUpdate;
            agent.DownloadAgentSuccess += OnDownloadAgentSuccess;
            agent.DownloadAgentFailure += OnDownloadAgentFailure;

            _agents.Add(agent);
        }

        /// <summary>
        /// Enqueues a new download task for the specified URL.
        /// </summary>
        /// <param name="downloadPath">Local file path where the downloaded data will be saved.</param>
        /// <param name="downloadUrl">Remote URL to download from.</param>
        /// <param name="userData">Optional user data attached to the task.</param>
        /// <returns>The created <see cref="DownloadTask"/>.</returns>
        public DownloadTask AddDownload(string downloadPath, string downloadUrl, object userData = null) {
            enabled = true;

            var downloadTask = new DownloadTask(downloadPath, downloadUrl, userData);
            _waitingTasks.AddLast(downloadTask);

            return downloadTask;
        }

        /// <summary>
        /// Removes and stops the download task identified by <paramref name="taskId"/>.
        /// Searches both the waiting queue and active agents.
        /// </summary>
        /// <param name="taskId">Identifier of the task to remove.</param>
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

        /// <summary>
        /// Cancels all pending and active downloads and disables the component.
        /// </summary>
        public void RemoveAllDownloads() {
            _waitingTasks.Clear();

            foreach (var agent in _agents) {
                agent.Stop();
            }

            enabled = false;
        }

        /// <summary>
        /// Retrieves a download task by its serial identifier.
        /// </summary>
        /// <param name="serialId">The task identifier to look up.</param>
        /// <returns>The matching <see cref="DownloadTask"/>, or null if not found.</returns>
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

        /// <summary>
        /// Pauses all download processing. Disables the component to stop
        /// per-frame updates while preserving agent state.
        /// </summary>
        public void Pause() {
            if (IsPaused) {
                return;
            }
            IsPaused = true;
            enabled = false;
        }

        /// <summary>
        /// Resumes download processing after a previous call to <see cref="Pause"/>.
        /// </summary>
        public void Resume() {
            if (!IsPaused) {
                return;
            }
            IsPaused = false;
            enabled = true;
        }

        /// <summary>
        /// Accumulates bytes downloaded by all agents and feeds them into
        /// the speed counter.
        /// </summary>
        private void UpdateDownloadSpeed() {
            for (var i = 0; i < _agents.Count; i++) {
                _speedCounter.AddDownloadSize(_agents[i].DownloadedSizeDelta);
            }
            _speedCounter.Update(Time.unscaledDeltaTime);
        }

        /// <summary>
        /// Polls each agent every frame: starts waiting tasks on idle agents,
        /// updates running agents, and stops completed agents.
        /// </summary>
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

        /// <summary>
        /// Dequeues the next waiting task and starts it on the given agent.
        /// </summary>
        /// <param name="agent">Agent to start the task on.</param>
        /// <returns>True if a task was started; false if the waiting queue is empty.</returns>
        private bool StartAnotherTask(IDownloadAgent agent) {
            if (_waitingTasks.Count > 0) {
                agent.Start(_waitingTasks.First.Value);
                _waitingTasks.RemoveFirst();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles the download-started callback from an agent.
        /// </summary>
        /// <param name="sender">The agent that raised the event.</param>
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

        /// <summary>
        /// Handles the download-progress callback from an agent.
        /// </summary>
        /// <param name="sender">The agent that raised the event.</param>
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

        /// <summary>
        /// Handles the download-success callback from an agent.
        /// </summary>
        /// <param name="sender">The agent that raised the event.</param>
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

        /// <summary>
        /// Handles the download-failure callback from an agent.
        /// </summary>
        /// <param name="sender">The agent that raised the event.</param>
        /// <param name="errorMessage">Description of the error that occurred.</param>
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

        /// <summary>
        /// Formats a byte-per-second speed value as a human-readable string
        /// using KB/s or MB/s units.
        /// </summary>
        /// <param name="speed">Speed in bytes per second.</param>
        /// <returns>A formatted speed string (e.g. "512.0KB/s").</returns>
        public static string FormatSpeed(float speed) {
            if (speed < 1024 * 1024) {
                return (speed / 1024).ToString("#0.0") + "KB/s";
            }

            return (speed / (1024 * 1024)).ToString("#0.0") + "MB/s";
        }
    }
}
