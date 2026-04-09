// ------------------------------------------------------------
//         File: Manifest.cs
//        Brief: Represents a patch manifest with version info, asset list, and diff utilities.
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
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using vFrame.Core;
using vFrame.Core.Unity;
using Version = System.Version;

namespace vFrame.Core.Unity
{
    public class Manifest
    {
        /// <summary>
        ///     Describes how an asset differs between two manifests.
        /// </summary>
        public enum DiffType
        {
            Added,
            Deleted,
            Modified
        }

        /// <summary>
        ///     Full assets list keyed by file name.
        /// </summary>
        private readonly Dictionary<string, AssetInfo> _assets = new Dictionary<string, AssetInfo>();

        /// <summary>
        ///     Parsed JSON manifest data.
        /// </summary>
        private ManifestJson _json;

        /// <summary>
        ///     The asset version.
        /// </summary>
        public Version AssetsVersion { get; private set; } = new Version(0, 0, 0);

        /// <summary>
        ///     The game engine version.
        /// </summary>
        public Version EngineVersion { get; private set; } = new Version(0, 0, 0);

        /// <summary>
        ///     The build number.
        /// </summary>
        public string BuildNumber => _json != null ? _json.buildNumber : null;

        /// <summary>
        ///     Whether the manifest has been fully loaded.
        /// </summary>
        public bool Loaded { get; private set; }

        /// <summary>
        ///     Whether the version information has been fully loaded.
        /// </summary>
        public bool VersionLoaded { get; private set; }

        /// <summary>
        ///     Gets a default manifest with empty version and loaded state.
        /// </summary>
        public static Manifest Default => new Manifest { Loaded = true, VersionLoaded = true };

        /// <summary>
        ///     Returns the full asset dictionary.
        /// </summary>
        /// <returns>Dictionary mapping file names to asset info.</returns>
        public Dictionary<string, AssetInfo> GetAssets() {
            return _assets;
        }

        /// <summary>
        ///     Parses the manifest file synchronously.
        /// </summary>
        /// <param name="manifestUrl">Path to the manifest JSON file.</param>
        public void Parse(string manifestUrl) {
            _json = LoadJson(manifestUrl);
            if (null == _json) {
                return;
            }

            LoadManifest();

            OnLoadJson(_json);
        }

        /// <summary>
        ///     Parses the manifest file asynchronously.
        /// </summary>
        /// <param name="manifestUrl">Path to the manifest JSON file.</param>
        /// <returns>Enumerator for coroutine scheduling.</returns>
        public IEnumerator ParseAsync(string manifestUrl) {
            var result = new Box<(bool, ManifestJson)>();
            yield return LoadJsonAsync(manifestUrl, result);

            var (success, json) = result.Value;
            if (!success) {
                yield break;
            }
            _json = json;

            LoadManifest();

            OnLoadJson(_json);
        }

        /// <summary>
        ///     Parses only the version portion of a manifest file synchronously.
        /// </summary>
        /// <param name="versionUrl">Path to the version JSON file.</param>
        public void ParseVersion(string versionUrl) {
            _json = LoadJson(versionUrl);
            if (_json == null) {
                return;
            }

            LoadVersion();

            OnLoadJson(_json);
        }

        /// <summary>
        ///     Parses only the version portion of a manifest file asynchronously.
        /// </summary>
        /// <param name="versionUrl">Path to the version JSON file.</param>
        /// <returns>Enumerator for coroutine scheduling.</returns>
        public IEnumerator ParseVersionAsync(string versionUrl) {
            var result = new Box<(bool, ManifestJson)>();
            yield return LoadJsonAsync(versionUrl, result);

            var (success, json) = result.Value;
            if (!success) {
                yield break;
            }
            _json = json;

            LoadVersion();

            OnLoadJson(_json);
        }

        /// <summary>
        ///     Compares this manifest's asset version to another.
        /// </summary>
        /// <param name="other">The manifest to compare against.</param>
        /// <returns>A value indicating the relative order of the versions.</returns>
        public int AssetsVersionCompareTo(Manifest other) {
            return AssetsVersion.CompareTo(other.AssetsVersion);
        }

        /// <summary>
        ///     Compares this manifest's engine version to another.
        /// </summary>
        /// <param name="other">The manifest to compare against.</param>
        /// <returns>A value indicating the relative order of the versions.</returns>
        public int GameVersionCompareTo(Manifest other) {
            return EngineVersion.CompareTo(other.EngineVersion);
        }

        /// <summary>
        ///     Sets the download state for a specific asset.
        /// </summary>
        /// <param name="fileName">File name of the asset.</param>
        /// <param name="state">The new download state.</param>
        public void SetAssetDownloadState(string fileName, DownloadState state) {
            AssetInfo asset;
            if (_assets.TryGetValue(fileName, out asset)) {
                asset.downloadState = state;
            }
        }

        /// <summary>
        ///     Generates a list of assets that have not yet been fully downloaded.
        /// </summary>
        /// <returns>List of assets pending download.</returns>
        public List<AssetInfo> GenResumeAssetsList() {
            var list = new List<AssetInfo>();

            foreach (var assetKV in _assets) {
                var asset = assetKV.Value;
                if (asset.downloadState < DownloadState.Downloaded) {
                    list.Add(asset);
                }
            }

            return list;
        }

        /// <summary>
        ///     Gets all assets that have been downloaded but not yet validated.
        /// </summary>
        /// <returns>List of downloaded assets awaiting validation.</returns>
        public List<AssetInfo> GetDownloadedAssets() {
            var list = new List<AssetInfo>();

            foreach (var assetKV in _assets) {
                var asset = assetKV.Value;
                if (asset.downloadState == DownloadState.Downloaded) {
                    list.Add(asset);
                }
            }

            return list;
        }

        /// <summary>
        ///     Gets all assets that have been downloaded and validated successfully.
        /// </summary>
        /// <returns>List of successfully downloaded assets.</returns>
        public List<AssetInfo> GetSucceedDownloadedAssets() {
            var list = new List<AssetInfo>();

            foreach (var assetKV in _assets) {
                var asset = assetKV.Value;
                if (asset.downloadState == DownloadState.Succeed) {
                    list.Add(asset);
                }
            }

            return list;
        }

        /// <summary>
        ///     Generates a dictionary of differences between this manifest and another.
        /// </summary>
        /// <param name="other">The manifest to diff against.</param>
        /// <returns>Dictionary mapping file names to their diff information.</returns>
        public Dictionary<string, AssetDiff> GenDiff(Manifest other) {
            var diffDic = new Dictionary<string, AssetDiff>();

            var otherAssets = other.GetAssets();
            foreach (var assetKV in _assets) {
                var key = assetKV.Key;
                var valueA = assetKV.Value;

                if (!otherAssets.ContainsKey(key)) {
                    var diff = new AssetDiff {
                        asset = valueA,
                        diffType = DiffType.Deleted
                    };
                    diffDic.Add(key, diff);
                    continue;
                }

                var valueB = otherAssets[key];
                if (valueA.md5 != valueB.md5) {
                    var diff = new AssetDiff {
                        asset = valueB,
                        diffType = DiffType.Modified
                    };
                    diffDic.Add(key, diff);
                }
            }

            foreach (var otherKV in otherAssets) {
                var key = otherKV.Key;
                var valueB = otherKV.Value;

                if (!_assets.ContainsKey(key)) {
                    var diff = new AssetDiff {
                        asset = valueB,
                        diffType = DiffType.Added
                    };
                    diffDic.Add(key, diff);
                }
            }

            return diffDic;
        }

        /// <summary>
        ///     Saves the manifest data to a JSON file.
        /// </summary>
        /// <param name="path">File path to save the manifest to.</param>
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
        ///     Loads and deserializes a manifest JSON file asynchronously.
        /// </summary>
        /// <param name="url">Path to the JSON file.</param>
        /// <param name="result">Box receiving the parsed result.</param>
        /// <returns>Enumerator for coroutine scheduling.</returns>
        private IEnumerator LoadJsonAsync(string url, Box<(bool, ManifestJson)> result) {
            Clear();

            var ret = new Box<(bool, string)>();
            yield return LoadTextFromFileAsync(url, ret);

            var (success, text) = ret.Value;
            if (!success) {
                result.Value = (false, null);
                yield break;
            }

            try {
                var json = JsonUtility.FromJson<ManifestJson>(text);
                result.Value = (true, json);
            }
            catch (Exception e) {
                result.Value = (false, null);
                Logger.Error(PatchConst.LogTag,
                    "Load json failed, url: {0}, text: {1}, message: {2}", url, text, e.Message);
            }
        }

        /// <summary>
        ///     Loads and deserializes a manifest JSON file synchronously.
        /// </summary>
        /// <param name="url">Path to the JSON file.</param>
        /// <returns>Parsed ManifestJson, or null on failure.</returns>
        private ManifestJson LoadJson(string url) {
            Clear();

            var text = string.Empty;
            try {
                text = File.ReadAllText(url);
                return JsonUtility.FromJson<ManifestJson>(text);
            }
            catch (Exception e) {
                Logger.Error(PatchConst.LogTag,
                    "Load json failed, url: {0}, text: {1}, message: {2}", url, text, e.Message);
                return null;
            }
        }

        /// <summary>
        ///     Extracts version numbers from the parsed JSON.
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
        ///     Loads version and asset data from the parsed JSON.
        /// </summary>
        private void LoadManifest() {
            LoadVersion();

            foreach (var asset in _json.assets) {
                _assets.Add(asset.fileName, asset);
            }

            Loaded = true;
        }

        /// <summary>
        ///     Override to handle additional JSON fields in derived manifest types.
        /// </summary>
        /// <param name="json">The parsed manifest JSON data.</param>
        protected virtual void OnLoadJson(ManifestJson json) { }

        /// <summary>
        ///     Resets all manifest state to default values.
        /// </summary>
        private void Clear() {
            _assets.Clear();
            _json = null;

            AssetsVersion = new Version(0, 0, 0);
            EngineVersion = new Version(0, 0, 0);

            Loaded = false;
            VersionLoaded = false;
        }

        /// <summary>
        ///     Reads text from a local file using UnityWebRequest.
        /// </summary>
        /// <param name="path">File path to read.</param>
        /// <param name="result">Box receiving the read result.</param>
        /// <returns>Enumerator for coroutine scheduling.</returns>
        private static IEnumerator LoadTextFromFileAsync(string path, Box<(bool, string)> result) {
            var fileUrl = path.NormalizeLocalFileURL();
            using (var webRequest = UnityWebRequest.Get(fileUrl)) {
                yield return webRequest.SendWebRequest();
                if (webRequest.isHttpError || webRequest.isNetworkError) {
                    Logger.Info(PatchConst.LogTag,
                        "Load text from file failed: {0}, error: {1}", path, webRequest.error);
                    result.Value = (false, string.Empty);
                    yield break;
                }

                var text = webRequest.downloadHandler.text;
                result.Value = (true, text);

                Logger.Info(PatchConst.LogTag,
                    "Load text from file succeed: {0}, text: {1}", path, text);
            }
        }

        /// <summary>
        ///     Describes the difference between two asset entries.
        /// </summary>
        public class AssetDiff
        {
            /// <summary>
            ///     The asset that differs.
            /// </summary>
            public AssetInfo asset;

            /// <summary>
            ///     The type of difference (added, deleted, or modified).
            /// </summary>
            public DiffType diffType;
        }
    }
}