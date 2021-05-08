using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using vFrame.Core.ThreadPools;
using Logger = vFrame.Core.Loggers.Logger;

namespace vFrame.Core.Patch
{
    public class HashChecker : MonoBehaviour
    {
        public static HashChecker Create(string storagePath) {
            var go = new GameObject("HashChecker") {hideFlags = HideFlags.HideAndDontSave};
            var instance = go.AddComponent<HashChecker>();
            instance._storagePath = storagePath;

            DontDestroyOnLoad(go);
            return instance;
        }

        private const int MaxThread = 3;

        private string _storagePath;
        private List<AssetInfo> _assets;

        public event Action OnCheckStarted;
        public event Action<AssetInfo, bool> OnCheckProgress;
        public event Action OnCheckFinished;

        public bool Valid { get; private set; }
        public int HashNum { get; private set; }
        public int HashTotal { get; private set; }
        public int HashFailedNum { get; private set; }

        private ThreadPool _threadPool;

        public void Check(List<AssetInfo> assets) {
            Valid = true;
            _assets = assets;
            HashNum = 0;
            HashTotal = _assets.Count;
            HashFailedNum = 0;

            _threadPool?.Destroy();
            _threadPool = new ThreadPool();
            _threadPool.Create(MaxThread);

            StopAllCoroutines();
            StartCoroutine(CO_Check());
        }

        private IEnumerator CO_Check() {
            yield return new WaitForSeconds(0.2f);

            if (OnCheckStarted != null) {
                OnCheckStarted();
            }

            Logger.Info(PatchConst.LogTag, "Validate file hash started, total count: {0}", HashTotal);

            for (HashNum = 1; HashNum <= HashTotal; HashNum++) {
                var asset = _assets[HashNum - 1];
                var filePath = _storagePath + asset.fileName;
                var process = new HashProcess(_threadPool, filePath);
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

            _threadPool?.Destroy();
            _threadPool = null;
        }

        private class HashProcess : CustomYieldInstruction
        {
            private readonly string _path;
            private readonly object _lockObject = new object();
            private bool _hashFinished;

            public Exception Error { get; private set; }
            public string HashValue { get; private set; }

            public HashProcess(ThreadPool threadPool, string path) {
                _path = path;
                _hashFinished = false;

                threadPool.AddTask(HashStream, null, OnHashError);
            }

            private void HashStream(object obj) {
                using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read)) {
                    HashValue = CalculateMd5(stream);
                }
                lock (_lockObject) {
                    _hashFinished = true;
                }
            }

            private void OnHashError(Exception e) {
                Error = e;

                lock (_lockObject) {
                    _hashFinished = true;
                }
            }

            private static string CalculateMd5(Stream stream) {
                using (var md5 = MD5.Create()) {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "");
                }
            }

            public override bool keepWaiting {
                get {
                    lock (_lockObject) {
                        return !_hashFinished;
                    }
                }
            }
        }
    }
}