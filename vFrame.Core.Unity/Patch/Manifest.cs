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

        /// <summary>
        /// Json Manifest format
        /// </summary>
        private ManifestJson _json = null;

        /// <summary>
        /// The asset version
        /// </summary>
        public Version AssetsVersion { get; private set; } = new Version(0, 0, 0);

        /// <summary>
        /// The game engine version
        /// </summary>
        public Version EngineVersion { get; private set; } = new Version(0, 0, 0);

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
        /// Get default manifest
        /// </summary>
        public static Manifest Default => new Manifest {Loaded = true, VersionLoaded = true};

        /// <summary>
        /// Parse the whole file, caller should check where the file exist
        /// </summary>
        public void Parse(string manifestUrl) {
            _json = LoadJson(manifestUrl);
            if (null == _json) {
                return;
            }

            LoadManifest();

            OnLoadJson(_json);
        }

        /// <summary>
        /// Parse the version part, caller should check where the file exist
        /// </summary>
        public void ParseVersion(string versionUrl) {
            _json = LoadJson(versionUrl);
            if (_json == null)
                return;

            LoadVersion();

            OnLoadJson(_json);
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

        /// <summary>
        /// Set download state
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="state"></param>
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

        /// <summary>
        /// Get downloaded assets (not validated)
        /// </summary>
        /// <returns></returns>
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
        /// Get succeed downloaded assets (validated)
        /// </summary>
        /// <returns></returns>
        public List<AssetInfo> GetSucceedDownloadedAssets() {
            var list = new List<AssetInfo>();

            foreach (var assetKV in _assets) {
                var asset = assetKV.Value;
                if (asset.downloadState == DownloadState.SUCCEED) {
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

        /// <summary>
        /// Save manifest to file
        /// </summary>
        /// <param name="path">File path to save</param>
        public void SaveToFile(string path) {
            var manifest = _json;
            if (null == manifest) {
                manifest = new ManifestJson {
                    buildNumber = BuildNumber ?? "0",
                    assetsVersion = AssetsVersion.ToString(),
                    engineVersion = EngineVersion.ToString(),
                    assets = new List<AssetInfo>(_assets.Values)
                };
            }
            else {
                _json.assets = new List<AssetInfo>(_assets.Values);
            }
            var jsonStr = JsonUtility.ToJson(manifest);
            File.WriteAllText(path, jsonStr, Encoding.UTF8);
        }

        /// <summary>
        /// Load json data from url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private ManifestJson LoadJson(string url) {
            Clear();

            try {
                var text = new FileReader().ReadAllText(url);
                return JsonUtility.FromJson<ManifestJson>(text);
            }
            catch (Exception e) {
                Logger.Error(PatchConst.LogTag, "Load json failed, url: {0}, message: {1}", url, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Load the version part
        /// </summary>
        private void LoadVersion() {
            AssetsVersion = string.IsNullOrEmpty(_json.assetsVersion)
                ? new Version("0.0.0")
                : new Version(_json.assetsVersion);
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

        /// <summary>
        /// On load json
        /// </summary>
        protected virtual void OnLoadJson(ManifestJson json) {

        }

        private void Clear() {
            _assets.Clear();
            _json = null;

            AssetsVersion = new Version(0, 0, 0);
            EngineVersion = new Version(0, 0, 0);

            Loaded = false;
            VersionLoaded = false;
        }
    }
}