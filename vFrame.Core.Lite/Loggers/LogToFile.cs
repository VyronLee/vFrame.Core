using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using vFrame.Core.Base;

namespace vFrame.Core.Loggers
{
    internal class LogToFile : BaseObject<string>
    {
        private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private Task _task;
        private string _logPath;

        // 每10s写入一次日志
        private const int WaitForMilliseconds = 10000;
        private CancellationTokenSource _cancellationTokenSource;

        protected override void OnCreate(string path) {
            CreateDirectory(path);

            _logPath = path;
            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Run(Update);
        }

        protected override void OnDestroy() {
            _cancellationTokenSource.Cancel();

            _task.Wait();
            _task.Dispose();
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
            using (var fileStream = File.OpenWrite(_logPath)) {
                using (var writer = new StreamWriter(fileStream)) {
                    var written = false;
                    while (_logQueue.TryDequeue(out var value)) {
                        writer.WriteLine(value);
                        written = true;
                    }
                    if (written) {
                        writer.Flush();
                    }
                }
            }
        }
    }
}
