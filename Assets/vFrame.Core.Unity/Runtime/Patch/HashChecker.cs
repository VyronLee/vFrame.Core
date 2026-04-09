// ------------------------------------------------------------
//         File: HashChecker.cs
//        Brief: Validates downloaded asset integrity via MD5 hash comparison.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-16 22:32:14
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;
using vFrame.Core.Unity;

namespace vFrame.Core.Unity
{
    public class HashChecker : MonoBehaviour
    {
        private const int MaxThread = 3;
        private List<AssetInfo> _assets;

        private string _storagePath;

        /// <summary>
        ///     Whether all checked assets passed hash validation.
        /// </summary>
        public bool Valid { get; private set; }

        /// <summary>
        ///     Number of assets that have been checked so far.
        /// </summary>
        public int HashNum { get; private set; }

        /// <summary>
        ///     Total number of assets to check.
        /// </summary>
        public int HashTotal { get; private set; }

        /// <summary>
        ///     Number of assets that failed hash validation.
        /// </summary>
        public int HashFailedNum { get; private set; }

        /// <summary>
        ///     Creates a new HashChecker instance attached to a hidden GameObject.
        /// </summary>
        /// <param name="storagePath">Root directory where downloaded assets are stored.</param>
        /// <returns>A configured HashChecker instance.</returns>
        public static HashChecker Create(string storagePath) {
            var go = new GameObject("HashChecker").DontDestroyEx().DontSaveAndHideEx();
            var instance = go.AddComponent<HashChecker>();
            instance._storagePath = storagePath;
            return instance;
        }

        /// <summary>
        ///     Raised when the hash check process starts.
        /// </summary>
        public event Action OnCheckStarted;

        /// <summary>
        ///     Raised for each asset after its hash has been computed.
        /// </summary>
        public event Action<AssetInfo, bool> OnCheckProgress;

        /// <summary>
        ///     Raised when all assets have been checked.
        /// </summary>
        public event Action OnCheckFinished;

        /// <summary>
        ///     Starts the hash validation process for the specified assets.
        /// </summary>
        /// <param name="assets">List of assets to validate.</param>
        public void Check(List<AssetInfo> assets) {
            Valid = true;
            _assets = assets;
            HashNum = 0;
            HashTotal = _assets.Count;
            HashFailedNum = 0;

            StopAllCoroutines();
            StartCoroutine(CheckInternal());
        }

        /// <summary>
        ///     Coroutine that iterates over assets and computes their MD5 hash.
        /// </summary>
        private IEnumerator CheckInternal() {
            yield return new WaitForSeconds(0.2f);

            if (OnCheckStarted != null) {
                OnCheckStarted();
            }

            Logger.Info(PatchConst.LogTag, "Validate file hash started, total count: {0}", HashTotal);

            for (HashNum = 1; HashNum <= HashTotal; HashNum++) {
                var asset = _assets[HashNum - 1];
                var filePath = _storagePath + asset.fileName;
                var process = new HashProcess(filePath);
                yield return process;

                if (process.Error != null) {
                    Logger.Error(PatchConst.LogTag, "Hash file failed: {0}, error: {1}", filePath, process.Error);
                    HashFailedNum++;
                    Valid = false;
                    continue;
                }

                var valid = string.Equals(process.HashValue, asset.md5, StringComparison.CurrentCultureIgnoreCase);
                if (OnCheckProgress != null) {
                    OnCheckProgress(asset, valid);
                }

                if (valid) {
                    Logger.Info(PatchConst.LogTag, "Hash file succeed: {0}.", filePath);
                    continue;
                }

                HashFailedNum++;
                Valid = false;

                Logger.Error(PatchConst.LogTag,
                    "Validate file hash failed, hash not match, file path: {0}, md5 desired: {1}, got: {2}",
                    filePath, asset.md5, process.HashValue);
            }

            Logger.Info(PatchConst.LogTag, "Validate file hash finished, failed count: {0}, total count: {1}",
                HashFailedNum, HashTotal);

            yield return null;

            if (OnCheckFinished != null) {
                OnCheckFinished();
            }
        }

        /// <summary>
        ///     Custom yield instruction that computes an MD5 hash on a background thread.
        /// </summary>
        private class HashProcess : CustomYieldInstruction
        {
            private readonly object _lockObject = new object();
            private readonly string _path;
            private bool _hashFinished;

            /// <summary>
            ///     Initializes a new HashProcess and queues the hash computation.
            /// </summary>
            /// <param name="path">Path to the file to hash.</param>
            public HashProcess(string path) {
                _path = path;
                _hashFinished = false;

                ThreadPool.QueueUserWorkItem(HashStream);
            }

            /// <summary>
            ///     Exception that occurred during hashing, if any.
            /// </summary>
            public Exception Error { get; private set; }

            /// <summary>
            ///     Computed hexadecimal MD5 hash string.
            /// </summary>
            public string HashValue { get; private set; }

            /// <summary>
            ///     Returns true while hashing is still in progress.
            /// </summary>
            public override bool keepWaiting {
                get {
                    lock (_lockObject) {
                        return !_hashFinished;
                    }
                }
            }

            /// <summary>
            ///     Reads the file and computes its MD5 hash on a thread-pool thread.
            /// </summary>
            /// <param name="obj">State object (unused).</param>
            private void HashStream(object obj) {
                try {
                    using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read)) {
                        HashValue = CalculateMd5(stream);
                    }
                }
                catch (Exception e) {
                    OnHashError(e);
                }

                lock (_lockObject) {
                    _hashFinished = true;
                }
            }

            /// <summary>
            ///     Records the exception and signals completion.
            /// </summary>
            /// <param name="e">The exception that occurred.</param>
            private void OnHashError(Exception e) {
                Error = e;

                lock (_lockObject) {
                    _hashFinished = true;
                }
            }

            /// <summary>
            ///     Computes an uppercase, dash-free hexadecimal MD5 hash from a stream.
            /// </summary>
            /// <param name="stream">The stream to hash.</param>
            /// <returns>Hexadecimal MD5 hash string.</returns>
            private static string CalculateMd5(Stream stream) {
                using (var md5 = MD5.Create()) {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }
        }
    }
}