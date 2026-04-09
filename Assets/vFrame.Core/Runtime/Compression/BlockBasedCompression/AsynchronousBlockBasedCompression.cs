// ------------------------------------------------------------
//         File: AsynchronousBlockBasedCompression.cs
//        Brief: Multi-threaded asynchronous block-based compression and decompression
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 22:55:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
// Compatibility-only dependency: the historical MultiThreading / Task runner remains here for
// legacy async compression flow, but it is not the preferred direction for new retained systems.

namespace vFrame.Core
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

        /// <summary>
        /// Releases the parallel task runner and cleans up resources.
        /// </summary>
        protected override void OnDestroy() {
            _parallelTaskRunner?.Destroy();
            _parallelTaskRunner = null;

            base.OnDestroy();
        }

        /// <summary>
        /// Sets the number of worker threads for parallel compression.
        /// Clamped to the range [0, <see cref="MaxThreadCount"/>].
        /// </summary>
        /// <param name="count">Desired thread count.</param>
        public void SetThreadCount(int count) {
            _threadCount = Math.Max(count, 0);
            _threadCount = Math.Min(count, MaxThreadCount);
        }

        /// <summary>
        /// Starts asynchronous compression of the input stream into the output stream.
        /// </summary>
        /// <param name="input">The input data stream to compress.</param>
        /// <param name="output">The output stream to receive compressed data.</param>
        /// <param name="options">Compression configuration options.</param>
        /// <returns>A request object that can be polled for completion.</returns>
        /// <exception cref="StateBusyException">Thrown when the compressor is not in an idle state.</exception>
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

        /// <summary>
        /// Compresses a single block as part of the parallel compression pipeline.
        /// </summary>
        /// <param name="state">The thread-local state containing block parameters.</param>
        private void CompressInternal(CompressThreadState state) {
            SafeCompress(state.Input, state.Output, state.Options, state.BlockIndex);

            state.Request.IncreaseFinishedCount();
        }

        /// <summary>
        /// Creates a callback delegate for compression completion.
        /// </summary>
        /// <param name="request">The compression request to finalize on completion.</param>
        /// <returns>An action delegate invoked when all blocks are compressed.</returns>
        private Action CompressedFinished(BlockBasedCompressionRequest request) {
            return () => OnCompressedFinished(request);
        }

        /// <summary>
        /// Finalizes the compression output stream and marks the request as done.
        /// </summary>
        /// <param name="request">The compression request to finalize.</param>
        private void OnCompressedFinished(BlockBasedCompressionRequest request) {
            EndCompress(_output);
            request.IsDone = true;
            _state = CompressionState.Idle;
        }

        /// <summary>
        /// Starts asynchronous decompression of the input stream into the output stream.
        /// </summary>
        /// <param name="input">The compressed data stream.</param>
        /// <param name="output">The output stream to receive decompressed data.</param>
        /// <returns>A request object that can be polled for completion.</returns>
        /// <exception cref="StateBusyException">Thrown when the compressor is not in an idle state.</exception>
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

        /// <summary>
        /// Decompresses a single block as part of the parallel decompression pipeline.
        /// </summary>
        /// <param name="state">The thread-local state containing block parameters.</param>
        private void DecompressInternal(DecompressThreadState state) {
            SafeDecompress(state.Input, state.Output, state.BlockIndex);

            state.Request.IncreaseFinishedCount();
        }

        /// <summary>
        /// Creates a callback delegate for decompression completion.
        /// </summary>
        /// <param name="request">The decompression request to finalize on completion.</param>
        /// <returns>An action delegate invoked when all blocks are decompressed.</returns>
        private Action DecompressedFinished(BlockBasedDecompressionRequest request) {
            return () => OnDecompressedFinished(request);
        }

        /// <summary>
        /// Finalizes the decompression output stream and marks the request as done.
        /// </summary>
        /// <param name="request">The decompression request to finalize.</param>
        private void OnDecompressedFinished(BlockBasedDecompressionRequest request) {
            EndDecompress(_output);
            request.IsDone = true;
            _state = CompressionState.Idle;
        }

        /// <summary>
        /// Handles errors from the parallel task runner by recording the last error
        /// and transitioning to the error state.
        /// </summary>
        /// <param name="e">The exception that occurred.</param>
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

            /// <summary>
            /// Initializes a new compression request bound to the given compressor instance.
            /// </summary>
            /// <param name="compression">The parent asynchronous compressor.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="compression"/> is null.</exception>
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

            /// <summary>
            /// Returns <c>true</c> while the request is not yet done.
            /// </summary>
            /// <returns><c>true</c> if the request is still in progress; otherwise <c>false</c>.</returns>
            public bool MoveNext() {
                return !IsDone;
            }

            /// <summary>
            /// Resets the enumerator. No-op for this implementation.
            /// </summary>
            public void Reset() { }

            public object Current => null;

            /// <summary>
            /// Atomically increments the finished block counter by one.
            /// </summary>
            public void IncreaseFinishedCount() {
                Interlocked.Add(ref _finishedCount, 1);
            }
        }


        public class BlockBasedDecompressionRequest : IEnumerator
        {
            private readonly AsynchronousBlockBasedCompression _compression;
            private int _finishedCount;

            private int _isDone;

            /// <summary>
            /// Initializes a new decompression request bound to the given compressor instance.
            /// </summary>
            /// <param name="compression">The parent asynchronous compressor.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="compression"/> is null.</exception>
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

            /// <summary>
            /// Returns <c>true</c> while the request is not yet done.
            /// </summary>
            /// <returns><c>true</c> if the request is still in progress; otherwise <c>false</c>.</returns>
            public bool MoveNext() {
                return !IsDone;
            }

            /// <summary>
            /// Resets the enumerator. No-op for this implementation.
            /// </summary>
            public void Reset() { }

            public object Current => null;

            /// <summary>
            /// Atomically increments the finished block counter by one.
            /// </summary>
            public void IncreaseFinishedCount() {
                Interlocked.Add(ref _finishedCount, 1);
            }
        }
    }
}