using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using vFrame.Core.FileReaders;
using Logger = vFrame.Core.Loggers.Logger;

namespace vFrame.Core.Patch
{
    public class Manifest
    {
        /// <summary>
        /// The type of difference
        /// </summary>
        public enum DiffType
        {
            ADDED,
            DELETED,
            MODIFIED
        }

        /// <summary>
        /// difference between 2 assets
        /// </summary>
        public class AssetDiff
        {
            public AssetInfo asset;
            public DiffType diffType;
        }

        private ManifestJson _json = null;

        /// <summary>
        /// The asset version
        /// </summary>
        public Version AssetsVersion { get; private set; }

        /// <summary>
        /// The game engine version
        /// </summary>
        public Version EngineVersion { get; private set; }

        /// <summary>
        /// The build number
        /// </summary>
        public string BuildNumber {
            get { return _json != null ? _json.buildNumber : null; }
        }

        /// <summary>
        /// Whether the manifest have been fully loaded
        /// </summary>
        public bool Loaded { get; private set; }

        /// <summary>
        /// Whether the version information have been fully loaded
        /// </summary>
        public bool VersionLoaded { get; private set; }

        /// <summary>
        /// Full assets list
        /// </summary>
        private readonly Dictionary<string, AssetInfo> _assets = new Dictionary<string, AssetInfo>();

        public Dictionary<string, AssetInfo> GetAssets() {
            return _assets;
        }

        /// <summary>
        /// Parse the whole file, caller should check where the file exist
        /// </summary>
        public void Parse(string manifestUrl) {
            LoadJson(manifestUrl);

            if (_json != null) {
                LoadManifest();
            }
        }

        /// <summary>
        /// Parse the version part, caller should check where the file exist
        /// </summary>
        public void ParseVersion(string versionUrl) {
            LoadJson(versionUrl);

            if (_json != null) {
                LoadVersion();
            }
        }

        /// <summary>
        /// assets version compare
        /// </summary>
        public int AssetsVersionCompareTo(Manifest other) {
            return AssetsVersion.CompareTo(other.AssetsVersion);
        }

        /// <summary>
        /// game version compare
        /// </summary>
        public int GameVersionCompareTo(Manifest other) {
            return EngineVersion.CompareTo(other.EngineVersion);
        }

        public void SetAssetDownloadState(string fileName, DownloadState state) {
            AssetInfo asset;
            if (_assets.TryGetValue(fileName, out asset)) {
                asset.downloadState = state;
            }
        }

        /// <summary>
        /// Generate resuming download assets list
        /// </summary>
        public List<AssetInfo> GenResumeAssetsList() {
            var list = new List<AssetInfo>();

            foreach (var assetKV in _assets) {
                var asset = assetKV.Value;
                if (asset.downloadState < DownloadState.DOWNLOADED) {
                    list.Add(asset);
                }
            }

            return list;
        }

        public List<AssetInfo> GetDownloadedAssets() {
            var list = new List<AssetInfo>();

            foreach (var assetKV in _assets) {
                var asset = assetKV.Value;
                if (asset.downloadState == DownloadState.DOWNLOADED) {
                    list.Add(asset);
                }
            }

            return list;
        }

        /// <summary>
        /// Generate difference between this Manifest and another
        /// </summary>
        public Dictionary<string, AssetDiff> GenDiff(Manifest other) {
            var diffDic = new Dictionary<string, AssetDiff>();

            var otherAssets = other.GetAssets();
            foreach (var assetKV in _assets) {
                var key = assetKV.Key;
                var valueA = assetKV.Value;

                // Deleted
                if (!otherAssets.ContainsKey(key)) {
                    var diff = new AssetDiff {
                        asset = valueA,
                        diffType = DiffType.DELETED
                    };
                    diffDic.Add(key, diff);
                    continue;
                }

                // Modified
                var valueB = otherAssets[key];
                if (valueA.md5 != valueB.md5) {
                    var diff = new AssetDiff {
                        asset = valueB,
                        diffType = DiffType.MODIFIED
                    };
                    diffDic.Add(key, diff);
                }
            }

            foreach (var otherKV in otherAssets) {
                var key = otherKV.Key;
                var valueB = otherKV.Value;

                // Added
                if (!_assets.ContainsKey(key)) {
                    var diff = new AssetDiff {
                        asset = valueB,
                        diffType = DiffType.ADDED
                    };
                    diffDic.Add(key, diff);
                }
            }

            return diffDic;
        }

        public void SaveToFile(string path) {
            _json.assets = new List<AssetInfo>(_assets.Values);
            var jsonStr = JsonUtility.ToJson(_json);
            File.WriteAllText(path, jsonStr, Encoding.UTF8);
        }

        private void LoadJson(string url) {
            Clear();

            try {
                var text = new FileReader().ReadAllText(url);
                _json = JsonUtility.FromJson<ManifestJson>(text);
            }
            catch (Exception e) {
                Logger.Error(PatchConst.LogTag, "Load json failed, url: {0}, message: {1}", url, e.Message);
            }
        }

        /// <summary>
        /// Load the version part
        /// </summary>
        private void LoadVersion() {
            AssetsVersion = new Version(_json.assetsVersion);
            EngineVersion = string.IsNullOrEmpty(_json.engineVersion)
                ? new Version("0.0.0")
                : new Version(_json.engineVersion);

            VersionLoaded = true;
        }

        /// <summary>
        /// Load all
        /// </summary>
        private void LoadManifest() {
            LoadVersion();

            foreach (var asset in _json.assets) {
                _assets.Add(asset.fileName, asset);
            }

            Loaded = true;
        }

        private void Clear() {
            _assets.Clear();
            _json = null;
            AssetsVersion = null;
            EngineVersion = null;

            Loaded = false;
            VersionLoaded = false;
        }
    }
}