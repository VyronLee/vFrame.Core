// ------------------------------------------------------------
//         File: LogToFile.cs
//        Brief: Asynchronous log file writer with rolling file support
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace vFrame.Core
{
    public class LogToFile : BaseObject<string>
    {
        private const int WaitForMilliseconds = 1000;
        private const int ShutdownTimeoutMs = 5000;

        private readonly object _lockObject = new object();
        private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private CancellationTokenSource _cancellationTokenSource;
        private DateTime _currentFileDate;
        private long _currentFileSize;
        private int _fileIndex;
        private FileStream _fileStream;
        private string _logPath;
        private LogToFileOptions _options;
        private Task _task;
        private StreamWriter _writer;

        public bool AppendTimestamp { get; set; }
        public string AppendTimestampFormat { get; set; } = "[yyyy-MM-dd HH:mm:ss.fff] ";

        /// <summary>
        ///     Called when the log file is created. Ensures the target directory exists,
        ///     opens the file handle, and starts the background flush task.
        /// </summary>
        /// <param name="path">The file path for the log output.</param>
        protected override void OnCreate(string path) {
            CreateDirectory(path);
            _logPath = path;
            _currentFileDate = DateTime.Today;
            _fileIndex = 0;
            OpenFileHandle();
            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Run(Update);
        }

        /// <summary>
        ///     Called when the log file is destroyed. Cancels the background task,
        ///     flushes remaining entries, and releases resources.
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
        ///     Configures rolling file options. Can be called after Create.
        /// </summary>
        /// <param name="options">The rolling file configuration.</param>
        public void Configure(LogToFileOptions options) {
            _options = options;
            if (options != null && options.FlushIntervalSeconds > 0) {
                // Note: interval is used by the background loop
            }
        }

        /// <summary>
        ///     Creates the directory for the given file path if it does not already exist.
        /// </summary>
        private static void CreateDirectory(string filePath) {
            var dirPath = Path.GetDirectoryName(filePath);
            if (dirPath != null) {
                Directory.CreateDirectory(dirPath);
            }
        }

        /// <summary>
        ///     Opens the file handle for appending. The handle stays open for the
        ///     lifetime of this object to avoid repeated open/close overhead.
        /// </summary>
        private void OpenFileHandle() {
            _fileStream = new FileStream(_logPath, FileMode.Append, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(_fileStream) { AutoFlush = false };
            _currentFileSize = _fileStream.Length;
        }

        /// <summary>
        ///     Closes and disposes the file handle.
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
        ///     Enqueues a log entry for asynchronous writing to the file.
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
        ///     Background loop that flushes queued log entries to disk at a fixed interval.
        ///     Exits when the cancellation token is triggered.
        /// </summary>
        private void Update() {
            var interval = _options?.FlushIntervalSeconds > 0
                ? _options.FlushIntervalSeconds * 1000
                : WaitForMilliseconds;

            while (!_cancellationTokenSource.Token.IsCancellationRequested) {
                WriteAllText();

                try {
                    Task.Delay(interval, _cancellationTokenSource.Token).Wait();
                }
                catch (AggregateException) {
                    // TaskCanceledException wrapped in AggregateException — exit loop
                    break;
                }
            }
        }

        /// <summary>
        ///     Dequeues all pending log entries and writes them to the log file.
        ///     Checks for rolling conditions before writing.
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
                        CheckRolling();
                        _writer.WriteLine(value);
                        _currentFileSize += value.Length + Environment.NewLine.Length;
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

        /// <summary>
        ///     Checks if the current log file needs rolling based on the configured strategy.
        /// </summary>
        private void CheckRolling() {
            if (_options == null || _options.Strategy == RollingStrategy.None) {
                return;
            }

            var needsRoll = false;

            if (_options.Strategy == RollingStrategy.BySize) {
                needsRoll = _currentFileSize >= _options.MaxFileSizeBytes;
            }
            else if (_options.Strategy == RollingStrategy.ByDate) {
                needsRoll = DateTime.Today != _currentFileDate;
            }

            if (needsRoll) {
                RollFile();
            }
        }

        /// <summary>
        ///     Rolls the current log file: closes the handle, renames the file with a
        ///     timestamp or index suffix, optionally compresses it, and opens a fresh file.
        /// </summary>
        private void RollFile() {
            CloseFileHandle();

            // Generate archive name
            var archivePath = GenerateArchivePath();
            try {
                if (File.Exists(_logPath)) {
                    File.Move(_logPath, archivePath);
                }
            }
            catch (IOException ex) {
                Debug.WriteLine($"[LogToFile] Failed to roll file: {ex.Message}");
            }

            // Compress archive if enabled
            if (_options?.CompressArchives == true) {
                CompressArchive(archivePath);
            }

            // Cleanup old archives if exceeding MaxFileCount
            CleanupOldArchives();

            // Reset state
            _currentFileDate = DateTime.Today;
            _currentFileSize = 0;

            // Reopen fresh file
            OpenFileHandle();
        }

        /// <summary>
        ///     Compresses the specified archive file using the configured compression algorithm
        ///     and deletes the original. On failure, keeps the uncompressed file to prevent data loss.
        /// </summary>
        private void CompressArchive(string archivePath) {
            if (!File.Exists(archivePath)) {
                return;
            }

            var compressionType = _options?.CompressionType ?? CompressorType.ZStd;
            var ext = GetCompressionExtension(compressionType);
            var compressedPath = archivePath + ext;
            try {
                using (var input = new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var output = new FileStream(compressedPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var compressor = CompressorPool.Instance().Rent(compressionType)) {
                    compressor.Compress(input, output);
                }

                File.Delete(archivePath);
            }
            catch (Exception ex) {
                Debug.WriteLine($"[LogToFile] Compression failed, keeping uncompressed: {ex.Message}");
                // Clean up partial compressed file if it exists
                try {
                    if (File.Exists(compressedPath)) {
                        File.Delete(compressedPath);
                    }
                }
                catch {
                    // Best effort cleanup
                }
            }
        }

        /// <summary>
        ///     Returns the file extension for the given compressor type.
        /// </summary>
        private static string GetCompressionExtension(CompressorType type) {
            switch (type) {
                case CompressorType.LZMA: return ".lzma";
                case CompressorType.LZ4: return ".lz4";
                case CompressorType.ZStd: return ".zst";
                case CompressorType.Zlib: return ".gz";
                default: return ".zst";
            }
        }

        /// <summary>
        ///     Generates the archive file path based on the rolling strategy.
        /// </summary>
        private string GenerateArchivePath() {
            var dir = Path.GetDirectoryName(_logPath) ?? "";
            var fileName = Path.GetFileNameWithoutExtension(_logPath);
            var ext = Path.GetExtension(_logPath);

            if (_options.Strategy == RollingStrategy.ByDate) {
                return Path.Combine(dir, $"{fileName}_{_currentFileDate:yyyy-MM-dd}{ext}");
            }

            // BySize: use index suffix
            _fileIndex++;
            return Path.Combine(dir, $"{fileName}.{_fileIndex:D3}{ext}");
        }

        /// <summary>
        ///     Removes old archive files when the count exceeds <see cref="LogToFileOptions.MaxFileCount" />.
        ///     Only considers files matching the base name pattern.
        /// </summary>
        private void CleanupOldArchives() {
            if (_options == null || _options.MaxFileCount <= 0) {
                return;
            }

            try {
                var dir = Path.GetDirectoryName(_logPath) ?? "";
                var fileName = Path.GetFileNameWithoutExtension(_logPath);
                var ext = Path.GetExtension(_logPath);

                // Match both uncompressed and compressed archive patterns
                var archives = Directory.GetFiles(dir, $"{fileName}_*{ext}")
                    .Concat(Directory.GetFiles(dir, $"{fileName}.*{ext}"))
                    .Concat(Directory.GetFiles(dir, $"{fileName}_*{ext}.lzma"))
                    .Concat(Directory.GetFiles(dir, $"{fileName}.*{ext}.lzma"))
                    .Concat(Directory.GetFiles(dir, $"{fileName}_*{ext}.lz4"))
                    .Concat(Directory.GetFiles(dir, $"{fileName}.*{ext}.lz4"))
                    .Concat(Directory.GetFiles(dir, $"{fileName}_*{ext}.zst"))
                    .Concat(Directory.GetFiles(dir, $"{fileName}.*{ext}.zst"))
                    .Concat(Directory.GetFiles(dir, $"{fileName}_*{ext}.gz"))
                    .Concat(Directory.GetFiles(dir, $"{fileName}.*{ext}.gz"))
                    .OrderBy(f => f)
                    .ToList();

                var filesToDelete = archives.Count - _options.MaxFileCount;
                for (var i = 0; i < filesToDelete; i++) {
                    try {
                        File.Delete(archives[i]);
                    }
                    catch (IOException ex) {
                        Debug.WriteLine($"[LogToFile] Failed to delete old archive: {ex.Message}");
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"[LogToFile] Cleanup failed: {ex.Message}");
            }
        }
    }
}