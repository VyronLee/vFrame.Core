// ------------------------------------------------------------
//         File: Patcher.cs
//        Brief: Orchestrates the patch update lifecycle: version check, download, and hash validation.
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
using System.Net;
using UnityEngine;
using vFrame.Core;
using vFrame.Core.Unity;
using Object = UnityEngine.Object;

namespace vFrame.Core.Unity
{
    public class Patcher
    {
        private const float UpdateProgressInterval = 0.2f;

        /// <summary>
        ///     The local path of cached manifest file
        /// </summary>
        private readonly string _cacheManifestPath;

        /// <summary>
        ///     The local path of cached version file
        /// </summary>
        private readonly string _cacheVersionPath;

        /// <summary>
        ///     Downloaded size for each asset
        /// </summary>
        private readonly Dictionary<string, ulong> _downloadedSize = new Dictionary<string, ulong>();

        /// <summary>
        ///     Download manager
        /// </summary>
        private readonly Downloader _downloader;

        /// <summary>
        ///     Options
        /// </summary>
        private readonly PatchOptions _options;

        /// <summary>
        ///     Remote version
        /// </summary>
        private readonly VersionManifest _remoteVersion = new VersionManifest();

        /// <summary>
        ///     The path to store downloaded resources
        /// </summary>
        private readonly string _storagePath;

        /// <summary>
        ///     Local temporary manifest for download resuming
        /// </summary>
        private readonly Manifest _tempManifest = new Manifest();

        /// <summary>
        ///     The local path of cached temporary manifest file
        /// </summary>
        private readonly string _tempManifestPath;

        /// <summary>
        ///     CdnUrl to download files from.
        /// </summary>
        private string _cdnUrl;

        /// <summary>
        ///     All assets unit to download
        /// </summary>
        private List<AssetInfo> _downloadUnits = new List<AssetInfo>();

        /// <summary>
        ///     All failed units
        /// </summary>
        private List<AssetInfo> _failedUnits = new List<AssetInfo>();

        /// <summary>
        ///     Hash checker
        /// </summary>
        private HashChecker _hashChecker;

        /// <summary>
        ///     Is patcher initialized?
        /// </summary>
        private bool _initialized;

        /// <summary>
        ///     Last update time of download progress.
        /// </summary>
        private float _lastUpdateTime;

        /// <summary>
        ///     Local manifest
        /// </summary>
        private Manifest _localManifest = new Manifest();

        /// <summary>
        ///     Remote manifest
        /// </summary>
        private Manifest _remoteManifest = new Manifest();

        /// <summary>
        ///     Total number of assets to download
        /// </summary>
        private int _totalToDownload;

        /// <summary>
        ///     Total number of assets still waiting to be downloaded
        /// </summary>
        private int _totalWaitToDownload;

        /// <summary>
        ///     Callback invoked to confirm an update before downloading starts.
        ///     Receives the total download size and a continuation action.
        /// </summary>
        public Action<ulong, Action> OnUpdateConfirm;

        /// <summary>
        ///     Initializes the patcher with the specified options.
        /// </summary>
        /// <param name="options">Configuration options for the patch process.</param>
        public Patcher(PatchOptions options) {
            _options = options;
            _storagePath = options.storagePath.NormalizeDirectoryPath();

            if (!Directory.Exists(_storagePath)) {
                Directory.CreateDirectory(_storagePath);
            }

            _cacheVersionPath = _storagePath + options.versionFilename;
            _cacheManifestPath = _storagePath + options.manifestFilename;
            _tempManifestPath = _storagePath + options.manifestFilename + ".tmp";

            _downloader = Downloader.Create("Patcher Download Manager");
            _downloader.Timeout = _options.timeout;
        }

        /// <summary>
        ///     Gets or sets the CDN URL. Prefers the remote version manifest value, then the option value.
        /// </summary>
        /// <exception cref="WebException">Thrown if no CDN URL is available.</exception>
        public string CdnUrl {
            get {
                if (!string.IsNullOrEmpty(_cdnUrl)) {
                    return _cdnUrl;
                }

                _cdnUrl = _options.cdnUrl;
                if (_remoteVersion.VersionLoaded && !string.IsNullOrEmpty(_remoteVersion.cdnUrl)) {
                    _cdnUrl = _remoteVersion.cdnUrl;
                }

                ThrowHelper.ThrowIfNullOrEmpty(_cdnUrl, "Option.cdnUrl");
                _cdnUrl = PathUtils.Combine(_cdnUrl, _options.cdnDir ?? string.Empty);
                return _cdnUrl;
            }
            set => _cdnUrl = value;
        }

        /// <summary>
        ///     Gets the full game download URL specified in the remote version manifest.
        /// </summary>
        public string DownloadUrl {
            get {
                if (_remoteVersion.VersionLoaded && !string.IsNullOrEmpty(_remoteVersion.downloadUrl)) {
                    return _remoteVersion.downloadUrl;
                }
                return string.Empty;
            }
        }

        /// <summary>
        ///     Gets the number of assets that have been hash-validated so far.
        /// </summary>
        public int HashNum => _hashChecker ? _hashChecker.HashNum : 0;

        /// <summary>
        ///     Gets the total number of assets to hash-validate.
        /// </summary>
        public int HashTotal => _hashChecker ? _hashChecker.HashTotal : 0;

        /// <summary>
        ///     Gets the local engine version string.
        /// </summary>
        public string EngineVersion => null != _localManifest ? _localManifest.EngineVersion.ToString() : string.Empty;

        /// <summary>
        ///     Gets the local assets version string.
        /// </summary>
        public string AssetsVersion => null != _localManifest ? _localManifest.AssetsVersion.ToString() : string.Empty;

        /// <summary>
        ///     Gets the remote engine version string.
        /// </summary>
        public string RemoteEngineVersion =>
            null != _remoteVersion ? _remoteVersion.EngineVersion.ToString() : string.Empty;

        /// <summary>
        ///     Gets the remote assets version string.
        /// </summary>
        public string RemoteAssetsVersion =>
            null != _remoteVersion ? _remoteVersion.AssetsVersion.ToString() : string.Empty;

        /// <summary>
        ///     Gets the total size in bytes that needs to be downloaded.
        /// </summary>
        public ulong TotalSize { get; private set; }

        /// <summary>
        ///     Gets the current download speed in bytes per second.
        /// </summary>
        public float DownloadSpeed => _downloader.Speed;

        /// <summary>
        ///     Gets whether the download is currently paused.
        /// </summary>
        public bool IsPaused => _downloader.IsPaused;

        /// <summary>
        ///     Gets the current update state.
        /// </summary>
        public UpdateState UpdateState { get; private set; } = UpdateState.Unchecked;

        /// <summary>
        ///     Event raised when the patch update state changes.
        /// </summary>
        public event Action<UpdateEvent> OnUpdateEvent;

        /// <summary>
        ///     Initializes the patcher by loading local and temporary manifests.
        /// </summary>
        /// <returns>Enumerator for coroutine scheduling.</returns>
        public IEnumerator Initialize() {
            yield return InitManifest();
            _initialized = true;
        }

        /// <summary>
        ///     Pauses the active download.
        /// </summary>
        public void Pause() {
            _downloader.Pause();
        }

        /// <summary>
        ///     Resumes a paused download.
        /// </summary>
        public void Resume() {
            _downloader.Resume();
        }

        /// <summary>
        ///     Stops all active downloads.
        /// </summary>
        public void Stop() {
            _downloader.RemoveAllDownloads();
        }

        /// <summary>
        ///     Returns the local manifest.
        /// </summary>
        /// <returns>The local Manifest instance.</returns>
        public Manifest GetLocalManifest() {
            return _localManifest;
        }

        /// <summary>
        ///     Returns the remote version manifest.
        /// </summary>
        /// <returns>The remote VersionManifest instance.</returns>
        public VersionManifest GetRemoteVersionManifest() {
            return _remoteVersion;
        }

        /// <summary>
        ///     Releases all resources held by the patcher.
        /// </summary>
        public void Release() {
            if (_hashChecker) {
                UnityEngine.Object.Destroy(_hashChecker.gameObject);
            }

            if (_downloader) {
                UnityEngine.Object.Destroy(_downloader.gameObject);
            }

            _initialized = false;
        }

        /// <summary>
        ///     Starts the update check by downloading the remote version file.
        /// </summary>
        public void CheckUpdate() {
            if (!_initialized) {
                Logger.Error(PatchConst.LogTag, "Patcher has not initialized.");
                return;
            }

            if (!_localManifest.Loaded) {
                Logger.Error(PatchConst.LogTag, "No local manifest file found.");
                DispatchUpdateEvent(UpdateEvent.EventCode.ErrorNoLocalManifest);
                return;
            }

            switch (UpdateState) {
                case UpdateState.Unchecked:
                    DownloadVersion();
                    break;
                case UpdateState.UpToDate:
                    DispatchUpdateEvent(UpdateEvent.EventCode.AlreadyUpToDate);
                    break;
                case UpdateState.NeedUpdate:
                case UpdateState.FailToUpdate:
                    DispatchUpdateEvent(UpdateEvent.EventCode.NewAssetsVersionFound);
                    break;
                case UpdateState.NeedForceUpdate:
                    DispatchUpdateEvent(UpdateEvent.EventCode.NewGameVersionFound);
                    break;
            }
        }

        /// <summary>
        ///     Starts downloading assets. Must call <see cref="CheckUpdate"/> first.
        /// </summary>
        public void StartUpdate() {
            if (!_initialized) {
                Logger.Error(PatchConst.LogTag, "Patcher has not initialized.");
                return;
            }

            if (!_localManifest.Loaded) {
                Logger.Error(PatchConst.LogTag, "No local manifest file found.");
                DispatchUpdateEvent(UpdateEvent.EventCode.ErrorNoLocalManifest);
                return;
            }

            switch (UpdateState) {
                case UpdateState.NeedUpdate:
                case UpdateState.NeedForceUpdate:
                    DownloadManifest();
                    break;
                case UpdateState.FailToUpdate:
                    DownloadFailedAssets();
                    break;
            }
        }

        #region private methods

        /// <summary>
        ///     Loads the local and temporary manifests.
        /// </summary>
        /// <returns>Enumerator for coroutine scheduling.</returns>
        private IEnumerator InitManifest() {
            yield return LoadLocalManifest();

            if (File.Exists(_tempManifestPath)) {
                _tempManifest.Parse(_tempManifestPath);
                if (!_tempManifest.Loaded) {
                    File.Delete(_tempManifestPath);
                }
            }
        }

        /// <summary>
        ///     Loads the local manifest from the streaming assets path or cache.
        /// </summary>
        /// <returns>Enumerator for coroutine scheduling.</returns>
        private IEnumerator LoadLocalManifest() {
            Manifest cachedManifest = null;
            if (File.Exists(_cacheManifestPath)) {
                Logger.Info(PatchConst.LogTag, $"Cache manifest found at path: {_cacheManifestPath}, parsing..");

                cachedManifest = new Manifest();
                cachedManifest.Parse(_cacheManifestPath);
                if (!cachedManifest.Loaded) {
                    File.Delete(_cacheManifestPath);
                    cachedManifest = null;
                }
            }

            var localManifestPath = Path.Combine(Application.streamingAssetsPath, _options.manifestFilename);
            var localVersionPath = Path.Combine(Application.streamingAssetsPath, _options.versionFilename);

            yield return _localManifest.ParseAsync(localManifestPath);
            if (!_localManifest.Loaded) {
                yield return _localManifest.ParseAsync(localVersionPath);
            }

            if (_localManifest.Loaded) {
                if (cachedManifest != null) {
                    var gvc = _localManifest.GameVersionCompareTo(cachedManifest);
                    var avc = _localManifest.AssetsVersionCompareTo(cachedManifest);

                    if ((gvc != 0 || avc > 0) && _options.deleteCacheOutOfDate) {
                        Logger.Info(PatchConst.LogTag,
                            $"Local version(engine: {_localManifest.EngineVersion} asset: {_localManifest.AssetsVersion}) greater than cache version(engine: {cachedManifest.EngineVersion} asset: {cachedManifest.AssetsVersion}), deleting storage path: {_storagePath}..");

                        Directory.Delete(_storagePath, true);
                        Directory.CreateDirectory(_storagePath);
                    }
                    else {
                        Logger.Info(PatchConst.LogTag,
                            $"Cache version(engine: {cachedManifest.EngineVersion} asset: {cachedManifest.AssetsVersion}) greater than local version(engine: {_localManifest.EngineVersion} asset: {_localManifest.AssetsVersion}), switching to cache manifest..");

                        _localManifest = cachedManifest;
                    }
                }
            }
            else {
                if (cachedManifest != null) {
                    _localManifest = cachedManifest;
                }
                else {
                    Logger.Info(PatchConst.LogTag, "Local manifest does not exist, generate default version..");
                    _localManifest = Manifest.Default;
                }
            }
        }

        /// <summary>
        ///     Downloads the remote version file and triggers version comparison.
        /// </summary>
        private void DownloadVersion() {
            var versionUrl = _options.versionUrl;
            if (string.IsNullOrEmpty(versionUrl)) {
                versionUrl = PathUtils.Combine(_options.cdnUrl, _options.cdnDir, _options.versionFilename);
            }

            if (string.IsNullOrEmpty(versionUrl)) {
                Logger.Error(PatchConst.LogTag, "Version url is empty, versionUrl or cdnUrl must be specified first!");
                return;
            }

            Logger.Info(PatchConst.LogTag, $"Start to download version file: {versionUrl}, to path: {_cacheVersionPath}");

            var task = _downloader.AddDownload(_cacheVersionPath, versionUrl);
            task.DownloadSuccess += args => {
                Logger.Info(PatchConst.LogTag, "Download version file succeed, parsing remote version..");
                ParseVersion();
            };
            task.DownloadFailure += args => {
                Logger.Warning(PatchConst.LogTag, $"Fail to download version: {versionUrl}, error: {args.Error}");
                DispatchUpdateEvent(UpdateEvent.EventCode.ErrorDownloadVersion);
                UpdateState = UpdateState.Unchecked;
            };

            UpdateState = UpdateState.DownloadingVersion;
        }

        /// <summary>
        ///     Parses the downloaded version file and determines the update state.
        /// </summary>
        private void ParseVersion() {
            _remoteVersion.ParseVersion(_cacheVersionPath);
            if (!_remoteVersion.VersionLoaded) {
                Logger.Error(PatchConst.LogTag, "Error parsing version.");

                UpdateState = UpdateState.Unchecked;
                DispatchUpdateEvent(UpdateEvent.EventCode.ErrorParseVersion);
            }
            else {
                var gvc = _localManifest.GameVersionCompareTo(_remoteVersion);
                if (gvc == 0) {
                    var avc = _localManifest.AssetsVersionCompareTo(_remoteVersion);
                    if (avc >= 0) {
                        UpdateState = UpdateState.UpToDate;
                        DispatchUpdateEvent(UpdateEvent.EventCode.AlreadyUpToDate);
                    }
                    else {
                        UpdateState = UpdateState.NeedUpdate;
                        DispatchUpdateEvent(UpdateEvent.EventCode.NewAssetsVersionFound);
                    }
                }
                else if (gvc < 0) {
                    UpdateState = UpdateState.NeedForceUpdate;
                    DispatchUpdateEvent(UpdateEvent.EventCode.NewGameVersionFound);
                }
                else {
                    Logger.Info(PatchConst.LogTag, $"Local Game Version({_localManifest.EngineVersion}) > Remote Game Version({_remoteVersion.EngineVersion})");

                    UpdateState = UpdateState.UpToDate;
                    DispatchUpdateEvent(UpdateEvent.EventCode.AlreadyUpToDate);
                }
            }
        }

        /// <summary>
        ///     Downloads the remote manifest file.
        /// </summary>
        private void DownloadManifest() {
            var url = PathUtils.Combine(CdnUrl, _options.manifestFilename);

            Logger.Info(PatchConst.LogTag, $"Start to download manifest file: {url}, to path: {_tempManifestPath}");

            var task = _downloader.AddDownload(_tempManifestPath, url);
            task.DownloadSuccess += args => {
                Logger.Info(PatchConst.LogTag, "Download manifest file succeed, parsing remote manifest..");
                ParseManifest();
            };
            task.DownloadFailure += args => {
                Logger.Warning(PatchConst.LogTag, $"Fail to download manifest: {url}, error: {args.Error}");
                DispatchUpdateEvent(UpdateEvent.EventCode.ErrorDownloadManifest);
                UpdateState = UpdateState.NeedUpdate;
            };

            UpdateState = UpdateState.DownloadingManifest;
        }

        /// <summary>
        ///     Parses the downloaded manifest file and starts the update process.
        /// </summary>
        private void ParseManifest() {
            _remoteManifest.Parse(_tempManifestPath);

            if (!_remoteManifest.Loaded) {
                Logger.Error(PatchConst.LogTag, "Error parsing manifest.");
                DispatchUpdateEvent(UpdateEvent.EventCode.ErrorParseManifest);
                UpdateState = UpdateState.NeedUpdate;
            }
            else {
                DoUpdate();
            }
        }

        /// <summary>
        ///     Computes the diff between local and remote manifests and starts downloading.
        /// </summary>
        private void DoUpdate() {
            UpdateState = UpdateState.Updating;

            _downloadUnits.Clear();
            _failedUnits.Clear();

            _totalToDownload = 0;
            _totalWaitToDownload = 0;
            TotalSize = 0;
            _downloadedSize.Clear();

            _downloader.RemoveAllDownloads();

            if (_tempManifest.Loaded &&
                _tempManifest.GameVersionCompareTo(_remoteManifest) == 0 &&
                _tempManifest.AssetsVersionCompareTo(_remoteManifest) == 0) {
                _remoteManifest = _tempManifest;
                _downloadUnits = _remoteManifest.GenResumeAssetsList();
                _totalWaitToDownload = _totalToDownload = _downloadUnits.Count;
                if (_totalWaitToDownload <= 0) {
                    OnDownloadUnitsFinished();
                    return;
                }

                TotalSize = CalculateTotalSize(_downloadUnits);
                _remoteManifest.SaveToFile(_tempManifestPath);

                var msg =
                    string.Format("Resuming from previous update, {0} assets remains to update.", _totalToDownload);
                Logger.Info(PatchConst.LogTag, msg);
                DispatchUpdateEvent(UpdateEvent.EventCode.UpdateProgression);

                if (OnUpdateConfirm != null) {
                    OnUpdateConfirm(TotalSize, BatchDownload);
                }
                else {
                    BatchDownload();
                }
            }
            else {
                var diffDic = _localManifest.GenDiff(_remoteManifest);

                var assets = _remoteManifest.GetAssets();
                foreach (var kv in assets) {
                    var assetName = kv.Key;
                    if (!diffDic.ContainsKey(assetName)) {
                        _remoteManifest.SetAssetDownloadState(assetName, DownloadState.Succeed);
                    }
                }
                _remoteManifest.SaveToFile(_tempManifestPath);

                if (diffDic.Count == 0) {
                    Logger.Info(PatchConst.LogTag, "No different files detected, update skip.");
                    UpdateSucceed();
                }
                else {
                    foreach (var kv in diffDic) {
                        var diff = kv.Value;
                        if (diff.diffType == Manifest.DiffType.Deleted) {
                            if (File.Exists(_storagePath + diff.asset.fileName)) {
                                File.Delete(_storagePath + diff.asset.fileName);
                            }
                        }
                        else {
                            _downloadUnits.Add(diff.asset);
                        }
                    }

                    _totalWaitToDownload = _totalToDownload = _downloadUnits.Count;
                    TotalSize = CalculateTotalSize(_downloadUnits);

                    var msg = string.Format("Start to update {0} assets.", _totalToDownload);
                    Logger.Info(PatchConst.LogTag, msg);
                    DispatchUpdateEvent(UpdateEvent.EventCode.UpdateProgression);

                    if (OnUpdateConfirm != null) {
                        OnUpdateConfirm(TotalSize, BatchDownload);
                    }
                    else {
                        BatchDownload();
                    }
                }
            }
        }

        /// <summary>
        ///     Sums the sizes of all assets in the list.
        /// </summary>
        /// <param name="assets">List of assets to total.</param>
        /// <returns>Total size in bytes, with a minimum of 1.</returns>
        private ulong CalculateTotalSize(List<AssetInfo> assets) {
            ulong size = 0;
            foreach (var asset in assets) {
                size += asset.size;
            }

            return size > 0 ? size : 1;
        }

        /// <summary>
        ///     Retries downloading all previously failed assets.
        /// </summary>
        private void DownloadFailedAssets() {
            if (_failedUnits.Count == 0) {
                return;
            }

            _downloadedSize.Clear();

            UpdateState = UpdateState.Updating;
            _downloadUnits.Clear();
            _downloadUnits = _failedUnits;
            _failedUnits = new List<AssetInfo>();
            _totalWaitToDownload = _totalToDownload = _downloadUnits.Count;
            TotalSize = CalculateTotalSize(_downloadUnits);

            var msg = string.Format("Start to update {0} failed assets.", _totalWaitToDownload);
            Logger.Info(PatchConst.LogTag, msg);
            DispatchUpdateEvent(UpdateEvent.EventCode.UpdateProgression);

            BatchDownload();
        }

        /// <summary>
        ///     Enqueues all pending download units with the downloader.
        /// </summary>
        private void BatchDownload() {
            if (_downloadUnits.Count <= 0) {
                OnDownloadUnitsFinished();
                return;
            }

            foreach (var asset in _downloadUnits) {
                var storagePath = _storagePath + asset.fileName;
                var url = PathUtils.Combine(CdnUrl, asset.fileName);

                var dir = Path.GetDirectoryName(storagePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }

                var task = _downloader.AddDownload(storagePath, url, asset);
                task.DownloadSuccess += OnDownloadSuccess;
                task.DownloadFailure += OnDownloadError;
                task.DownloadUpdate += OnDownloadProgress;
            }
        }

        /// <summary>
        ///     Called when all download units have completed (success or failure).
        /// </summary>
        private void OnDownloadUnitsFinished() {
            Logger.Info(PatchConst.LogTag, $"Download Finish - {_failedUnits.Count} download failed.");

            _downloader.RemoveAllDownloads();

            if (_failedUnits.Count > 0) {
                UpdateFailed(UpdateEvent.EventCode.ErrorDownloadFailed);
            }
            else {
                var assets = _remoteManifest.GetDownloadedAssets();
                ValidateAssets(assets);
            }
        }

        /// <summary>
        ///     Marks the update as failed with the specified event code.
        /// </summary>
        /// <param name="code">Event code indicating the failure reason.</param>
        private void UpdateFailed(UpdateEvent.EventCode code = UpdateEvent.EventCode.UpdateFailed) {
            _remoteManifest.SaveToFile(_tempManifestPath);
            UpdateState = UpdateState.FailToUpdate;
            DispatchUpdateEvent(code);
        }

        /// <summary>
        ///     Finalizes a successful update by promoting the temporary manifest.
        /// </summary>
        private void UpdateSucceed() {
            _localManifest = _remoteManifest;
            _remoteManifest = null;

            if (File.Exists(_cacheManifestPath)) {
                File.Delete(_cacheManifestPath);
            }

            File.Move(_tempManifestPath, _cacheManifestPath);

            UpdateState = UpdateState.UpToDate;
            DispatchUpdateEvent(UpdateEvent.EventCode.UpdateFinished);
        }

        /// <summary>
        ///     Handles a single asset download success.
        /// </summary>
        /// <param name="args">Download event arguments.</param>
        private void OnDownloadSuccess(DownloadEventArgs args) {
            var asset = (AssetInfo)args.UserData;
            var task = _downloader.GetDownload(args.SerialId);

            Logger.Info(PatchConst.LogTag, $"Download file succeed: {asset.fileName}, url: {task?.DownloadUrl ?? string.Empty}, storage path: {task?.DownloadPath ?? string.Empty}");

            _remoteManifest.SetAssetDownloadState(asset.fileName, DownloadState.Downloaded);
            _remoteManifest.SaveToFile(_tempManifestPath);

            _totalWaitToDownload--;
            RecordDownloadedSize(asset.fileName, asset.size);
            DispatchUpdateEvent(UpdateEvent.EventCode.UpdateProgression);
            DispatchUpdateEvent(UpdateEvent.EventCode.AssetUpdated, asset.fileName);

            if (_totalWaitToDownload <= 0) {
                OnDownloadUnitsFinished();
            }
        }

        /// <summary>
        ///     Handles download progress updates, throttled by interval.
        /// </summary>
        /// <param name="args">Download event arguments with progress info.</param>
        private void OnDownloadProgress(DownloadEventArgs args) {
            var asset = (AssetInfo)args.UserData;
            RecordDownloadedSize(asset.fileName, args.DownloadedSize);

            if (Time.realtimeSinceStartup - _lastUpdateTime < UpdateProgressInterval) {
                return;
            }

            DispatchUpdateEvent(UpdateEvent.EventCode.UpdateProgression);
            _lastUpdateTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        ///     Tracks the downloaded size for an asset.
        /// </summary>
        /// <param name="assetName">File name of the asset.</param>
        /// <param name="size">Bytes downloaded so far.</param>
        private void RecordDownloadedSize(string assetName, ulong size) {
            if (_downloadedSize.ContainsKey(assetName)) {
                _downloadedSize[assetName] = size;
            }
            else {
                _downloadedSize.Add(assetName, size);
            }
        }

        /// <summary>
        ///     Handles a single asset download failure.
        /// </summary>
        /// <param name="args">Download event arguments with error info.</param>
        private void OnDownloadError(DownloadEventArgs args) {
            var asset = (AssetInfo)args.UserData;
            var task = _downloader.GetDownload(args.SerialId);
            Logger.Warning(PatchConst.LogTag, $"Download file failed: {asset.fileName}, url: {task?.DownloadUrl ?? string.Empty}, storage path: {task?.DownloadPath ?? string.Empty}, error: {args.Error}");

            _totalWaitToDownload--;
            _failedUnits.Add(asset);

            if (_totalWaitToDownload <= 0) {
                OnDownloadUnitsFinished();
            }
        }

        /// <summary>
        ///     Dispatches an update event to all subscribers.
        /// </summary>
        /// <param name="code">The event code to dispatch.</param>
        /// <param name="assetName">Optional asset name associated with the event.</param>
        private void DispatchUpdateEvent(UpdateEvent.EventCode code, string assetName = "") {
            if (OnUpdateEvent == null) {
                return;
            }

            var evt = new UpdateEvent { Code = code, AssetName = assetName };

            if (code == UpdateEvent.EventCode.UpdateProgression) {
                evt.DownloadedSize = CalculateDownloadedSize();
                evt.Percent = (float)evt.DownloadedSize / TotalSize;
                evt.PercentByFile = (float)(_totalToDownload - _totalWaitToDownload - _failedUnits.Count) /
                                    _totalToDownload;
            }

            if (OnUpdateEvent != null) {
                OnUpdateEvent(evt);
            }
        }

        /// <summary>
        ///     Sums all recorded download sizes.
        /// </summary>
        /// <returns>Total downloaded bytes.</returns>
        private ulong CalculateDownloadedSize() {
            ulong size = 0;
            foreach (var kv in _downloadedSize) {
                size += kv.Value;
            }

            return size;
        }

        /// <summary>
        ///     Starts hash validation for the downloaded assets.
        /// </summary>
        /// <param name="assets">List of assets to validate.</param>
        private void ValidateAssets(List<AssetInfo> assets) {
            if (!_hashChecker) {
                _hashChecker = HashChecker.Create(_storagePath);
                _hashChecker.OnCheckStarted += OnCheckStarted;
                _hashChecker.OnCheckProgress += OnCheckProgress;
                _hashChecker.OnCheckFinished += OnCheckFinished;
            }

            _hashChecker.Check(assets);
        }

        /// <summary>
        ///     Handles the hash check start event.
        /// </summary>
        private void OnCheckStarted() {
            DispatchUpdateEvent(UpdateEvent.EventCode.HashStart);
        }

        /// <summary>
        ///     Handles a hash check progress event for a single asset.
        /// </summary>
        /// <param name="asset">The asset that was checked.</param>
        /// <param name="valid">Whether the asset's hash matched.</param>
        private void OnCheckProgress(AssetInfo asset, bool valid) {
            if (valid) {
                _remoteManifest.SetAssetDownloadState(asset.fileName, DownloadState.Succeed);
            }
            else {
                _remoteManifest.SetAssetDownloadState(asset.fileName, DownloadState.NotStarted);
                _failedUnits.Add(asset);

                Logger.Warning(PatchConst.LogTag, $"Hash Invalid : {asset.fileName}");
            }

            _remoteManifest.SaveToFile(_tempManifestPath);

            DispatchUpdateEvent(UpdateEvent.EventCode.HashProgression);
        }

        /// <summary>
        ///     Handles the hash check completion event.
        /// </summary>
        private void OnCheckFinished() {
            if (_hashChecker && _hashChecker.Valid) {
                UpdateSucceed();
            }
            else {
                UpdateFailed(UpdateEvent.EventCode.HashValidationFailed);
            }
        }

        #endregion
    }
}