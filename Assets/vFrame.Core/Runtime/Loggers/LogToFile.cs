using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using vFrame.Core.Base;

namespace vFrame.Core.Loggers
{
    public class LogToFile : BaseObject<string>
    {
        // 每10s写入一次日志
        private const int WaitForMilliseconds = 10000;
        private readonly object _lockObject = new object();
        private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private CancellationTokenSource _cancellationTokenSource;
        private string _logPath;
        private Task _task;

        public bool AppendTimestamp { get; set; }
        public string AppendTimestampFormat { get; set; } = "[yyyy-MM-dd HH:mm:ss.fff] ";

        protected override void OnCreate(string path) {
            CreateDirectory(path);

            _logPath = path;
            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Run(Update);
        }

        protected override void OnDestroy() {
            _cancellationTokenSource.Cancel();

            _task?.Wait();
            _task?.Dispose();
            _task = null;

            // Flush before quit.
            WriteAllText();
        }

        private static void CreateDirectory(string filePath) {
            var dirPath = Path.GetDirectoryName(filePath);
            if (dirPath != null) {
                Directory.CreateDirectory(dirPath);
            }
        }

        public void AppendText(string value) {
            if (AppendTimestamp) {
                value = DateTime.Now.ToString(AppendTimestampFormat) + value;
            }
            _logQueue.Enqueue(value);
        }

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
                    // Nothing to do
                }
            }
        }

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