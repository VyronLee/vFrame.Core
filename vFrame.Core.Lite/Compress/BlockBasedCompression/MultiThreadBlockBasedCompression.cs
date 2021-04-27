using System;
using System.Collections;
using System.IO;
using vFrame.Core.ThreadPools;

namespace vFrame.Core.Compress.BlockBasedCompression
{
    public class MultiThreadBlockBasedCompression : BlockBasedCompression
    {
        private enum CompressionState
        {
            Idle,
            Compressing,
            Decompressing,
            Error,
        }

        private const int DefaultThreadCount = 3;
        private const int MaxThreadCount = 16;

        private ThreadPool _threadPool;
        private CompressionState _state = CompressionState.Idle;
        private Stream _input;
        private Stream _output;
        private Exception _lastError;

        public Exception LastError => _lastError;

        protected override void OnCreate() {
            base.OnCreate();

            _threadPool = new ThreadPool();
            _threadPool.Create(DefaultThreadCount);
        }

        protected override void OnDestroy() {
            _threadPool?.Destroy();
            _threadPool = null;

            base.OnDestroy();
        }

        public void SetThreadCount(int count) {
            count = Math.Max(count, 0);
            count = Math.Min(count, MaxThreadCount);

            _threadPool?.Destroy();
            _threadPool = new ThreadPool();
            _threadPool.Create(count);
        }

        public BlockBasedCompressionRequest CompressAsync(
            Stream input,
            Stream output,
            BlockBasedCompressionOptions options)
        {
            if (_state != CompressionState.Idle) {
                throw new BlockBasedCompressionStateBusyException();
            }

            _state = CompressionState.Compressing;
            _input = input;
            _output = output;

            BeginCompress(input, output, options);

            var request = new BlockBasedCompressionRequest(this) {TotalCount = BlockCount};

            for (var i = 0; i < BlockCount; i++) {
                var stateContext = new CompressThreadState {
                    Input = input,
                    Ouput = output,
                    Options = options,
                    Request = request,
                    BlockIndex = i
                };
                _threadPool.AddTask(CompressInternal, stateContext, OnException);
            }

            return request;
        }

        private void CompressInternal(object threadState) {
            if (!(threadState is CompressThreadState state)) {
                return;
            }

            SafeCompress(state.Input, state.Ouput, state.Options, state.BlockIndex);

            state.Request.IncreaseFinishedCount();
        }

        private void OnCompressedFinished() {
            EndCompress(_output);
            _state = CompressionState.Idle;
        }

        public BlockBasedDecompressionRequest DecompressAsync(
            Stream input,
            Stream output)
        {
            if (_state != CompressionState.Idle) {
                throw new BlockBasedCompressionStateBusyException();
            }

            _state = CompressionState.Decompressing;
            _input = input;
            _output = output;

            BeginDecompress(input, output);

            var request = new BlockBasedDecompressionRequest(this) {TotalCount = BlockCount};

            for (var i = 0; i < BlockCount; i++) {
                var stateContext = new DecompressThreadState {
                    Input = input,
                    Ouput = output,
                    Request = request,
                    BlockIndex = i
                };
                _threadPool.AddTask(DecompressInternal, stateContext, OnException);
            }

            return request;
        }

        private void DecompressInternal(object threadState) {
            if (!(threadState is DecompressThreadState state)) {
                return;
            }

            SafeDecompress(state.Input, state.Ouput, state.BlockIndex);

            state.Request.IncreaseFinishedCount();
        }

        private void OnDecompressedFinished() {
            EndDecompress(_output);
            _state = CompressionState.Idle;
        }

        private void OnException(Exception e) {
            _lastError = e;
            _state = CompressionState.Error;
        }

        private class CompressThreadState
        {
            public Stream Input { get; set; }
            public Stream Ouput { get; set; }
            public int BlockIndex { get; set; }
            public BlockBasedCompressionOptions Options { get; set; }
            public BlockBasedCompressionRequest Request { get; set; }
        }

        private class DecompressThreadState
        {
            public Stream Input { get; set; }
            public Stream Ouput { get; set; }
            public int BlockIndex { get; set; }
            public BlockBasedDecompressionRequest Request { get; set; }
        }

        public class BlockBasedCompressionRequest : IEnumerator
        {
            public int FinishedCount { get; set; }
            public int TotalCount { get; set; }
            public bool IsDone { get; private set; }
            public Exception Error => _compression?.LastError;

            private readonly MultiThreadBlockBasedCompression _compression;

            public BlockBasedCompressionRequest(MultiThreadBlockBasedCompression compression) {
                _compression = compression ?? throw new ArgumentNullException(nameof(compression));
            }

            public void IncreaseFinishedCount() {
                lock (this) {
                    if (++FinishedCount < TotalCount) {
                        return;
                    }
                }

                IsDone = true;

                _compression.OnCompressedFinished();
            }

            public bool MoveNext() {
                return !IsDone;
            }

            public void Reset() {
            }

            public object Current => null;
        }


        public class BlockBasedDecompressionRequest : IEnumerator
        {
            public int FinishedCount { get; set; }
            public int TotalCount { get; set; }
            public bool IsDone { get; private set; }
            public Exception Error => _compression?.LastError;

            private readonly MultiThreadBlockBasedCompression _compression;

            public BlockBasedDecompressionRequest(MultiThreadBlockBasedCompression compression) {
                _compression = compression ?? throw new ArgumentNullException(nameof(compression));
            }

            public void IncreaseFinishedCount() {
                lock (this) {
                    if (++FinishedCount < TotalCount) {
                        return;
                    }
                }

                IsDone = true;

                _compression.OnDecompressedFinished();
            }

            public bool MoveNext() {
                return !IsDone;
            }

            public void Reset() {
            }

            public object Current => null;
        }
    }
}