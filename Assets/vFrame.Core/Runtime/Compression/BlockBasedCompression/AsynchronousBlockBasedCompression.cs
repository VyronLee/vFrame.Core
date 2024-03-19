using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using vFrame.Core.Exceptions;
using vFrame.Core.MultiThreading;

namespace vFrame.Core.Compression
{
    public class AsynchronousBlockBasedCompression : BlockBasedCompression
    {
        private const int DefaultThreadCount = 3;
        private const int MaxThreadCount = 16;
        private Stream _input;
        private Stream _output;

        private ParallelTaskRunner<CompressThreadState> _parallelTaskRunner;

        private CompressionState _state = CompressionState.Idle;
        private int _threadCount = DefaultThreadCount;

        public Exception LastError { get; private set; }

        protected override void OnDestroy() {
            _parallelTaskRunner?.Destroy();
            _parallelTaskRunner = null;

            base.OnDestroy();
        }

        public void SetThreadCount(int count) {
            _threadCount = Math.Max(count, 0);
            _threadCount = Math.Min(count, MaxThreadCount);
        }

        public BlockBasedCompressionRequest CompressAsync(
            Stream input,
            Stream output,
            BlockBasedCompressionOptions options) {
            if (_state != CompressionState.Idle) {
                throw new StateBusyException();
            }

            _state = CompressionState.Compressing;
            _input = input;
            _output = output;

            BeginCompress(input, output, options);

            var request = new BlockBasedCompressionRequest(this) { TotalCount = BlockCount };

            var contexts = new List<CompressThreadState>(BlockCount);
            for (var i = 0; i < BlockCount; i++) {
                var stateContext = new CompressThreadState {
                    Input = input,
                    Output = output,
                    Options = options,
                    Request = request,
                    BlockIndex = i
                };
                contexts.Add(stateContext);
            }
            ParallelTaskRunner<CompressThreadState>.Spawn(_threadCount)
                .OnHandle(CompressInternal)
                .OnComplete(CompressedFinished(request))
                .OnError(OnException)
                .Run(contexts);

            return request;
        }

        private void CompressInternal(CompressThreadState state) {
            SafeCompress(state.Input, state.Output, state.Options, state.BlockIndex);

            state.Request.IncreaseFinishedCount();
        }

        private Action CompressedFinished(BlockBasedCompressionRequest request) {
            return () => OnCompressedFinished(request);
        }

        private void OnCompressedFinished(BlockBasedCompressionRequest request) {
            EndCompress(_output);
            request.IsDone = true;
            _state = CompressionState.Idle;
        }

        public BlockBasedDecompressionRequest DecompressAsync(
            Stream input,
            Stream output) {
            if (_state != CompressionState.Idle) {
                throw new StateBusyException();
            }

            _state = CompressionState.Decompressing;
            _input = input;
            _output = output;

            BeginDecompress(input, output);

            var request = new BlockBasedDecompressionRequest(this) { TotalCount = BlockCount };

            var contexts = new List<DecompressThreadState>(BlockCount);
            for (var i = 0; i < BlockCount; i++) {
                var stateContext = new DecompressThreadState {
                    Input = input,
                    Output = output,
                    Request = request,
                    BlockIndex = i
                };
                contexts.Add(stateContext);
            }
            ParallelTaskRunner<DecompressThreadState>.Spawn(_threadCount)
                .OnHandle(DecompressInternal)
                .OnComplete(DecompressedFinished(request))
                .OnError(OnException)
                .Run(contexts);

            return request;
        }

        private void DecompressInternal(DecompressThreadState state) {
            SafeDecompress(state.Input, state.Output, state.BlockIndex);

            state.Request.IncreaseFinishedCount();
        }

        private Action DecompressedFinished(BlockBasedDecompressionRequest request) {
            return () => OnDecompressedFinished(request);
        }

        private void OnDecompressedFinished(BlockBasedDecompressionRequest request) {
            EndDecompress(_output);
            request.IsDone = true;
            _state = CompressionState.Idle;
        }

        private void OnException(Exception e) {
            LastError = e;
            _state = CompressionState.Error;
        }

        private enum CompressionState
        {
            Idle,
            Compressing,
            Decompressing,
            Error
        }

        private class CompressThreadState
        {
            public Stream Input { get; set; }
            public Stream Output { get; set; }
            public int BlockIndex { get; set; }
            public BlockBasedCompressionOptions Options { get; set; }
            public BlockBasedCompressionRequest Request { get; set; }
        }

        private class DecompressThreadState
        {
            public Stream Input { get; set; }
            public Stream Output { get; set; }
            public int BlockIndex { get; set; }
            public BlockBasedDecompressionRequest Request { get; set; }
        }

        public class BlockBasedCompressionRequest : IEnumerator
        {
            private readonly AsynchronousBlockBasedCompression _compression;
            private int _finishedCount;

            private int _isDone;

            public BlockBasedCompressionRequest(AsynchronousBlockBasedCompression compression) {
                ThrowHelper.ThrowIfNull(compression, nameof(compression));
                _compression = compression;
            }

            public int FinishedCount => _finishedCount;

            public int TotalCount { get; set; }

            public bool IsDone {
                get => _isDone > 0;
                set => Interlocked.Exchange(ref _isDone, value ? 1 : 0);
            }

            public Exception Error => _compression?.LastError;

            public bool MoveNext() {
                return !IsDone;
            }

            public void Reset() { }

            public object Current => null;

            public void IncreaseFinishedCount() {
                Interlocked.Add(ref _finishedCount, 1);
            }
        }


        public class BlockBasedDecompressionRequest : IEnumerator
        {
            private readonly AsynchronousBlockBasedCompression _compression;
            private int _finishedCount;

            private int _isDone;

            public BlockBasedDecompressionRequest(AsynchronousBlockBasedCompression compression) {
                ThrowHelper.ThrowIfNull(compression, nameof(compression));
                _compression = compression;
            }

            public int FinishedCount => _finishedCount;

            public int TotalCount { get; set; }

            public bool IsDone {
                get => _isDone > 0;
                set => Interlocked.Exchange(ref _isDone, value ? 1 : 0);
            }

            public Exception Error => _compression?.LastError;

            public bool MoveNext() {
                return !IsDone;
            }

            public void Reset() { }

            public object Current => null;

            public void IncreaseFinishedCount() {
                Interlocked.Add(ref _finishedCount, 1);
            }
        }
    }
}