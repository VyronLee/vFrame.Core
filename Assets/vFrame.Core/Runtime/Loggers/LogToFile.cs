// ------------------------------------------------------------
//         File: LogToFile.cs
//        Brief: Asynchronous log file writer that flushes
//               buffered entries to disk at a fixed interval.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2018-10-20 18:09:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using vFrame.Core;

namespace vFrame.Core
{
    public class LogToFile : BaseObject<string>
    {
        private const int WaitForMilliseconds = 1000;
        private const int ShutdownTimeoutMs = 5000;

        private readonly object _lockObject = new object();
        private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private CancellationTokenSource _cancellationTokenSource;
        private string _logPath;
        private FileStream _fileStream;
        private StreamWriter _writer;
        private Task _task;

        public bool AppendTimestamp { get; set; }
        public string AppendTimestampFormat { get; set; } = "[yyyy-MM-dd HH:mm:ss.fff] ";

        /// <summary>
        /// Called when the log file is created. Ensures the target directory exists,
        /// opens the file handle, and starts the background flush task.
        /// </summary>
        /// <param name="path">The file path for the log output.</param>
        protected override void OnCreate(string path) {
            CreateDirectory(path);

            _logPath = path;
            OpenFileHandle();
            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Run(Update);
        }

        /// <summary>
        /// Called when the log file is destroyed. Cancels the background task,
        /// flushes remaining entries, and releases resources.
        /// </summary>
        protected override void OnDestroy() {
            if (_cancellationTokenSource != null) {
                _cancellationTokenSource.Cancel();
            }

            if (_task != null) {
                try {
                    _task.Wait(ShutdownTimeoutMs);
                }
                catch (AggregateException) {
                    // Task may throw if cancellation race with WriteAllText
                }
                _task.Dispose();
                _task = null;
            }

            // Final flush to ensure no data loss
            WriteAllText();
            CloseFileHandle();

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        /// <summary>
        /// Creates the directory for the given file path if it does not already exist.
        /// </summary>
        private static void CreateDirectory(string filePath) {
            var dirPath = Path.GetDirectoryName(filePath);
            if (dirPath != null) {
                Directory.CreateDirectory(dirPath);
            }
        }

        /// <summary>
        /// Opens the file handle for appending. The handle stays open for the
        /// lifetime of this object to avoid repeated open/close overhead.
        /// </summary>
        private void OpenFileHandle() {
            _fileStream = new FileStream(_logPath, FileMode.Append, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(_fileStream) { AutoFlush = false };
        }

        /// <summary>
        /// Closes and disposes the file handle.
        /// </summary>
        private void CloseFileHandle() {
            try {
                _writer?.Flush();
                _writer?.Dispose();
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex) {
                Debug.WriteLine($"[LogToFile] Error disposing writer: {ex.Message}");
            }
            _writer = null;

            try {
                _fileStream?.Dispose();
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex) {
                Debug.WriteLine($"[LogToFile] Error disposing file stream: {ex.Message}");
            }
            _fileStream = null;
        }

        /// <summary>
        /// Enqueues a log entry for asynchronous writing to the file.
        /// </summary>
        /// <param name="value">The log text to append.</param>
        /// <param name="urgent">If true, flushes to disk immediately (for Error/Fatal).</param>
        public void AppendText(string value, bool urgent = false) {
            if (AppendTimestamp) {
                value = DateTime.Now.ToString(AppendTimestampFormat) + value;
            }
            _logQueue.Enqueue(value);

            if (urgent) {
                WriteAllText();
            }
        }

        /// <summary>
        /// Background loop that flushes queued log entries to disk at a fixed interval.
        /// Exits when the cancellation token is triggered.
        /// </summary>
        private void Update() {
            while (!_cancellationTokenSource.Token.IsCancellationRequested) {
                WriteAllText();

                try {
                    Task.Delay(WaitForMilliseconds, _cancellationTokenSource.Token).Wait();
                }
                catch (AggregateException) {
                    // TaskCanceledException wrapped in AggregateException — exit loop
                    break;
                }
            }
        }

        /// <summary>
        /// Dequeues all pending log entries and writes them to the log file.
        /// </summary>
        private void WriteAllText() {
            lock (_lockObject) {
                if (_writer == null) {
                    // File handle not available — drain queue to prevent unbounded growth
                    while (_logQueue.TryDequeue(out _)) { }
                    return;
                }

                try {
                    while (_logQueue.TryDequeue(out var value)) {
                        _writer.WriteLine(value);
                    }
                    _writer.Flush();
                }
                catch (ObjectDisposedException) {
                    // File was closed during shutdown — drain remaining queue
                    while (_logQueue.TryDequeue(out _)) { }
                }
                catch (Exception ex) {
                    Debug.WriteLine($"[LogToFile] Write failed: {ex.Message}");
                }
            }
        }
    }
}
