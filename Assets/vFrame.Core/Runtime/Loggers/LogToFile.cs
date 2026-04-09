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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using vFrame.Core;

namespace vFrame.Core
{
    public class LogToFile : BaseObject<string>
    {
        private const int WaitForMilliseconds = 10000;
        private readonly object _lockObject = new object();
        private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private CancellationTokenSource _cancellationTokenSource;
        private string _logPath;
        private Task _task;

        public bool AppendTimestamp { get; set; }
        public string AppendTimestampFormat { get; set; } = "[yyyy-MM-dd HH:mm:ss.fff] ";

        /// <summary>
        /// Called when the log file is created. Ensures the target directory exists
        /// and starts the background flush task.
        /// </summary>
        /// <param name="path">The file path for the log output.</param>
        protected override void OnCreate(string path) {
            CreateDirectory(path);

            _logPath = path;
            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Run(Update);
        }

        /// <summary>
        /// Called when the log file is destroyed. Cancels the background task,
        /// flushes remaining entries, and releases resources.
        /// </summary>
        protected override void OnDestroy() {
            _cancellationTokenSource.Cancel();

            _task?.Wait();
            _task?.Dispose();
            _task = null;

            WriteAllText();
        }

        /// <summary>
        /// Creates the directory for the given file path if it does not already exist.
        /// </summary>
        /// <param name="filePath">The file path whose parent directory should be created.</param>
        private static void CreateDirectory(string filePath) {
            var dirPath = Path.GetDirectoryName(filePath);
            if (dirPath != null) {
                Directory.CreateDirectory(dirPath);
            }
        }

        /// <summary>
        /// Enqueues a log entry for asynchronous writing to the file.
        /// </summary>
        /// <param name="value">The log text to append.</param>
        public void AppendText(string value) {
            if (AppendTimestamp) {
                value = DateTime.Now.ToString(AppendTimestampFormat) + value;
            }
            _logQueue.Enqueue(value);
        }

        /// <summary>
        /// Background loop that flushes queued log entries to disk at a fixed interval.
        /// Exits when the cancellation token is triggered.
        /// </summary>
        private async void Update() {
            while (true) {
                WriteAllText();

                try {
                    await Task.Delay(WaitForMilliseconds, _cancellationTokenSource.Token);
                }
                catch (TaskCanceledException) {
                    break;
                }
                catch (Exception) {
                }
            }
        }

        /// <summary>
        /// Dequeues all pending log entries and writes them to the log file.
        /// </summary>
        private void WriteAllText() {
            lock (_lockObject) {
                using (var fileStream = File.OpenWrite(_logPath)) {
                    fileStream.Seek(0, SeekOrigin.End);
                    using (var writer = new StreamWriter(fileStream)) {
                        while (_logQueue.TryDequeue(out var value)) {
                            writer.WriteLine(value);
                        }
                        writer.Flush();
                    }
                }
            }
        }
    }
}
