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
        private StreamWriter _writer;
        private Task _task;

        // 每10s写入一次日志
        private const int WaitForMilliseconds = 10000;
        private CancellationTokenSource _cancellationTokenSource;

        protected override void OnCreate(string path) {
            CreateDirectory(path);

            var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            _writer = new StreamWriter(fileStream);
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

            _writer?.Dispose();
            _writer = null;
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
            var writen = false;
            while (_logQueue.TryDequeue(out var value)) {
                _writer.WriteLine(value);
                writen = true;
            }
            if (writen) {
                _writer.Flush();
            }
        }
    }
}
