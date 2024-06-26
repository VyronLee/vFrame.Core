﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using vFrame.Core.Exceptions;
using vFrame.Core.Unity.Download;
using vFrame.Core.Unity.Utils;
using Logger = vFrame.Core.Loggers.Logger;
using Object = UnityEngine.Object;

namespace vFrame.Core.Unity.Patch
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
        ///     Update confirm callback.
        /// </summary>
        public Action<ulong, Action> OnUpdateConfirm;

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
        ///     Return cdnUrl, prefer value specified in remoteVersion manifest, then option's.
        /// </summary>
        /// <exception cref="WebException"></exception>
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
        ///     Return downloadUrl specified in remoteVersion manifest.
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
        ///     Return current validated hash number.
        /// </summary>
        public int HashNum => _hashChecker ? _hashChecker.HashNum : 0;

        /// <summary>
        ///     Return total hash number.
        /// </summary>
        public int HashTotal => _hashChecker ? _hashChecker.HashTotal : 0;

        /// <summary>
        ///     Return local engine version.
        /// </summary>
        public string EngineVersion => null != _localManifest ? _localManifest.EngineVersion.ToString() : string.Empty;

        /// <summary>
        ///     Return local assets version.
        /// </summary>
        public string AssetsVersion => null != _localManifest ? _localManifest.AssetsVersion.ToString() : string.Empty;

        /// <summary>
        ///     Return remote engine version.
        /// </summary>
        public string RemoteEngineVersion =>
            null != _remoteVersion ? _remoteVersion.EngineVersion.ToString() : string.Empty;

        /// <summary>
        ///     Return remote assets version.
        /// </summary>
        public string RemoteAssetsVersion =>
            null != _remoteVersion ? _remoteVersion.AssetsVersion.ToString() : string.Empty;

        /// <summary>
        ///     Total size need to download
        /// </summary>
        public ulong TotalSize { get; private set; }

        /// <summary>
        ///     Return current download speed
        /// </summary>
        public float DownloadSpeed => _downloader.Speed;

        /// <summary>
        ///     Is Download paused?
        /// </summary>
        public bool IsPaused => _downloader.IsPaused;

        /// <summary>
        ///     Return current update state.
        /// </summary>
        public UpdateState UpdateState { get; private set; } = UpdateState.Unchecked;

        /// <summary>
        ///     Update event callback.
        /// </summary>
        public event Action<UpdateEvent> OnUpdateEvent;

        /// <summary>
        ///     Initial patcher.
        /// </summary>
        /// <returns></returns>
        public IEnumerator Initialize() {
            yield return InitManifest();
            _initialized = true;
        }

        /// <summary>
        ///     Pause download.
        /// </summary>
        public void Pause() {
            _downloader.Pause();
        }

        /// <summary>
        ///     Resume download.
        /// </summary>
        public void Resume() {
            _downloader.Resume();
        }

        /// <summary>
        ///     Stop download.
        /// </summary>
        public void Stop() {
            _downloader.RemoveAllDownloads();
        }

        /// <summary>
        ///     Return local manifest
        /// </summary>
        /// <returns></returns>
        public Manifest GetLocalManifest() {
            return _localManifest;
        }

        /// <summary>
        ///     Return remote version manifest
        /// </summary>
        /// <returns></returns>
        public VersionManifest GetRemoteVersionManifest() {
            return _remoteVersion;
        }

        /// <summary>
        ///     Release patcher.
        /// </summary>
        public void Release() {
            if (_hashChecker) {
                Object.Destroy(_hashChecker.gameObject);
            }

            if (_downloader) {
                Object.Destroy(_downloader.gameObject);
            }

            _initialized = false;
        }

        /// <summary>
        ///     Check for update
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
        ///     Start update from remote, must call CheckUpdate first
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

        private IEnumerator InitManifest() {
            // local
            yield return LoadLocalManifest();

            // temp
            if (File.Exists(_tempManifestPath)) {
                _tempManifest.Parse(_tempManifestPath);
                if (!_tempManifest.Loaded) {
                    File.Delete(_tempManifestPath);
                }
            }
        }

        private IEnumerator LoadLocalManifest() {
            // Find the cached manifest file
            Manifest cachedManifest = null;
            if (File.Exists(_cacheManifestPath)) {
                Logger.Info(PatchConst.LogTag, "Cache manifest found at path: {0}, parsing..", _cacheManifestPath);

                cachedManifest = new Manifest();
                cachedManifest.Parse(_cacheManifestPath);
                if (!cachedManifest.Loaded) {
                    File.Delete(_cacheManifestPath);
                    cachedManifest = null;
                }
            }

            // Load local manifest in app package
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
                            "Local version(engine: {0} asset: {1}) greater than cache version(engine: {2} asset: {3}), deleting storage path: {4}..",
                            _localManifest.EngineVersion,
                            _localManifest.AssetsVersion,
                            cachedManifest.EngineVersion,
                            cachedManifest.AssetsVersion,
                            _storagePath);

                        Directory.Delete(_storagePath, true);
                        Directory.CreateDirectory(_storagePath);
                    }
                    else {
                        Logger.Info(PatchConst.LogTag,
                            "Cache version(engine: {0} asset: {1}) greater than local version(engine: {2} asset: {3}), switching to cache manifest..",
                            cachedManifest.EngineVersion,
                            cachedManifest.AssetsVersion,
                            _localManifest.EngineVersion,
                            _localManifest.AssetsVersion);

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

        private void DownloadVersion() {
            var versionUrl = _options.versionUrl;
            if (string.IsNullOrEmpty(versionUrl)) {
                versionUrl = PathUtils.Combine(_options.cdnUrl, _options.cdnDir, _options.versionFilename);
            }

            if (string.IsNullOrEmpty(versionUrl)) {
                Logger.Error(PatchConst.LogTag, "Version url is empty, versionUrl or cdnUrl must be specified first!");
                return;
            }

            Logger.Info(PatchConst.LogTag, "Start to download version file: {0}, to path: {1}", versionUrl,
                _cacheVersionPath);

            var task = _downloader.AddDownload(_cacheVersionPath, versionUrl);
            task.DownloadSuccess += args => {
                Logger.Info(PatchConst.LogTag, "Download version file succeed, parsing remote version..");
                ParseVersion();
            };
            task.DownloadFailure += args => {
                Logger.Warning(PatchConst.LogTag, "Fail to download version: {0}, error: {1}", versionUrl, args.Error);
                DispatchUpdateEvent(UpdateEvent.EventCode.ErrorDownloadVersion);
                UpdateState = UpdateState.Unchecked;
            };

            UpdateState = UpdateState.DownloadingVersion;
        }

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
                    Logger.Info(PatchConst.LogTag, "Local Game Version({0}) > Remote Game Version({1})",
                        _localManifest.EngineVersion, _remoteVersion.EngineVersion);

                    UpdateState = UpdateState.UpToDate;
                    DispatchUpdateEvent(UpdateEvent.EventCode.AlreadyUpToDate);
                }
            }
        }

        private void DownloadManifest() {
            var url = PathUtils.Combine(CdnUrl, _options.manifestFilename);

            Logger.Info(PatchConst.LogTag, "Start to download manifest file: {0}, to path: {1}", url,
                _tempManifestPath);

            var task = _downloader.AddDownload(_tempManifestPath, url);
            task.DownloadSuccess += args => {
                Logger.Info(PatchConst.LogTag, "Download manifest file succeed, parsing remote manifest..");
                ParseManifest();
            };
            task.DownloadFailure += args => {
                Logger.Warning(PatchConst.LogTag, "Fail to download manifest: {0}, error: {1}", url, args.Error);
                DispatchUpdateEvent(UpdateEvent.EventCode.ErrorDownloadManifest);
                UpdateState = UpdateState.NeedUpdate;
            };

            UpdateState = UpdateState.DownloadingManifest;
        }

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

        private void DoUpdate() {
            UpdateState = UpdateState.Updating;

            _downloadUnits.Clear();
            _failedUnits.Clear();

            _totalToDownload = 0;
            _totalWaitToDownload = 0;
            TotalSize = 0;
            _downloadedSize.Clear();

            _downloader.RemoveAllDownloads();

            // Temporary manifest exists, resuming previous download
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
                // Temporary manifest not exists or out of date,
                var diffDic = _localManifest.GenDiff(_remoteManifest);

                // Save assets state which has not changed.
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

        private ulong CalculateTotalSize(List<AssetInfo> assets) {
            ulong size = 0;
            foreach (var asset in assets) {
                size += asset.size;
            }

            return size > 0 ? size : 1;
        }

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

        private void OnDownloadUnitsFinished() {
            Logger.Info(PatchConst.LogTag, "Download Finish - {0} download failed.", _failedUnits.Count);

            // Release file locks.
            _downloader.RemoveAllDownloads();

            if (_failedUnits.Count > 0) {
                UpdateFailed(UpdateEvent.EventCode.ErrorDownloadFailed);
            }
            else {
                var assets = _remoteManifest.GetDownloadedAssets();
                ValidateAssets(assets);
            }
        }

        private void UpdateFailed(UpdateEvent.EventCode code = UpdateEvent.EventCode.UpdateFailed) {
            _remoteManifest.SaveToFile(_tempManifestPath);
            UpdateState = UpdateState.FailToUpdate;
            DispatchUpdateEvent(code);
        }

        private void UpdateSucceed() {
            _localManifest = _remoteManifest;
            _remoteManifest = null;

            // rename temporary manifest to valid manifest
            if (File.Exists(_cacheManifestPath)) {
                File.Delete(_cacheManifestPath);
            }

            File.Move(_tempManifestPath, _cacheManifestPath);

            UpdateState = UpdateState.UpToDate;
            DispatchUpdateEvent(UpdateEvent.EventCode.UpdateFinished);
        }

        private void OnDownloadSuccess(DownloadEventArgs args) {
            var asset = (AssetInfo)args.UserData;
            var task = _downloader.GetDownload(args.SerialId);

            Logger.Info(PatchConst.LogTag, "Download file succeed: {0}, url: {1}, storage path: {2}",
                asset.fileName,
                task?.DownloadUrl ?? string.Empty,
                task?.DownloadPath ?? string.Empty);

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

        private void OnDownloadProgress(DownloadEventArgs args) {
            var asset = (AssetInfo)args.UserData;
            RecordDownloadedSize(asset.fileName, args.DownloadedSize);

            if (Time.realtimeSinceStartup - _lastUpdateTime < UpdateProgressInterval) {
                return;
            }

            DispatchUpdateEvent(UpdateEvent.EventCode.UpdateProgression);
            _lastUpdateTime = Time.realtimeSinceStartup;
        }

        private void RecordDownloadedSize(string assetName, ulong size) {
            if (_downloadedSize.ContainsKey(assetName)) {
                _downloadedSize[assetName] = size;
            }
            else {
                _downloadedSize.Add(assetName, size);
            }
        }

        private void OnDownloadError(DownloadEventArgs args) {
            var asset = (AssetInfo)args.UserData;
            var task = _downloader.GetDownload(args.SerialId);
            Logger.Warning(PatchConst.LogTag, "Download file failed: {0}, url: {1}, storage path: {2}, error: {3}",
                asset.fileName,
                task?.DownloadUrl ?? string.Empty,
                task?.DownloadPath ?? string.Empty,
                args.Error);

            _totalWaitToDownload--;
            _failedUnits.Add(asset);
            //DispatchUpdateEvent(UpdateEvent.EventCode.ERROR_DOWNLOAD_FAILED, asset.fileName);

            if (_totalWaitToDownload <= 0) {
                OnDownloadUnitsFinished();
            }
        }

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

        private ulong CalculateDownloadedSize() {
            ulong size = 0;
            foreach (var kv in _downloadedSize) {
                size += kv.Value;
            }

            return size;
        }

        private void ValidateAssets(List<AssetInfo> assets) {
            if (!_hashChecker) {
                _hashChecker = HashChecker.Create(_storagePath);
                _hashChecker.OnCheckStarted += OnCheckStarted;
                _hashChecker.OnCheckProgress += OnCheckProgress;
                _hashChecker.OnCheckFinished += OnCheckFinished;
            }

            _hashChecker.Check(assets);
        }

        private void OnCheckStarted() {
            DispatchUpdateEvent(UpdateEvent.EventCode.HashStart);
        }

        private void OnCheckProgress(AssetInfo asset, bool valid) {
            if (valid) {
                _remoteManifest.SetAssetDownloadState(asset.fileName, DownloadState.Succeed);
            }
            else {
                _remoteManifest.SetAssetDownloadState(asset.fileName, DownloadState.NotStarted);
                _failedUnits.Add(asset);

                Logger.Warning(PatchConst.LogTag, "Hash Invalid : {0}", asset.fileName);
            }

            _remoteManifest.SaveToFile(_tempManifestPath);

            DispatchUpdateEvent(UpdateEvent.EventCode.HashProgression);
        }

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