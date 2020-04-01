using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using vFrame.Core.Loggers;
using Logger = vFrame.Core.Loggers.Logger;

namespace vFrame.Core.Patch
{
    public class HashChecker : MonoBehaviour
    {
        public static HashChecker Create(string storagePath) {
            var go = new GameObject("HashChecker");
            var instance = go.AddComponent<HashChecker>();
            instance._storagePath = storagePath;

            return instance;
        }

        private const int NUM_PER_FRAME = 5;

        private string _storagePath;
        private List<AssetInfo> _assets;

        public event Action OnCheckStarted;
        public event Action<AssetInfo, bool> OnCheckProgress;
        public event Action OnCheckFinished;

        public bool Valid { get; private set; }
        public int HashNum { get; private set; }
        public int HashTotal { get; private set; }

        public void Check(List<AssetInfo> assets) {
            Valid = true;
            _assets = assets;
            HashNum = 0;
            HashTotal = _assets.Count;

            StopAllCoroutines();
            StartCoroutine(CO_Check());
        }

        private IEnumerator CO_Check() {
            yield return new WaitForSeconds(0.2f);

            if (OnCheckStarted != null) {
                OnCheckStarted();
            }

            for (HashNum = 1; HashNum <= HashTotal; HashNum++) {
                var asset = _assets[HashNum - 1];
                var valid = HashValid(asset);

                if (OnCheckProgress != null) {
                    OnCheckProgress(asset, valid);
                }

                if (!valid)
                    Valid = false;

                if (HashNum % NUM_PER_FRAME == 0)
                    yield return null;
            }

            yield return null;
            if (OnCheckFinished != null) {
                OnCheckFinished();
            }
        }

        private bool HashValid(AssetInfo asset) {
            var fileName = _storagePath + asset.fileName;

            try {
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read)) {
                    return CalculateMd5(fileStream) == asset.md5;
                }
            }
            catch (Exception e) {
                Logger.Info(new LogTag("AssetsUpdater"), e.Message);
                return false;
            }
        }

        private static string CalculateMd5(Stream stream) {
            using (var md5 = MD5.Create()) {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}