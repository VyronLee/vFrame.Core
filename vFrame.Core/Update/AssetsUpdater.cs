using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using vFrame.Core.Download;
using vFrame.Core.Loggers;
using Logger = vFrame.Core.Loggers.Logger;
using Object = UnityEngine.Object;

namespace vFrame.Core.Update
{
    public partial class AssetsUpdater
    {
        private static readonly LogTag AssetsUpdaterLogTag = new LogTag("AssetsUpdater");

        private const float UPDATE_PROGRESS_INTERVAL = 0.2f;

        private const string VERSION_FILENAME = "GameVersion.json.unity3d";
        private const string TEMP_MANIFEST_FILENAME = "GameAssets.json.temp";
        private const string MANIFEST_FILENAME = "GameAssets.json.unity3d";

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

        private readonly HashChecker _hashChecker;

        /// <summary>
        ///     hot fix url
        /// </summary>
        private readonly string _hotFixUrl;

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
        ///     All assets unit to download
        /// </summary>
        private List<Manifest.AssetInfo> _downloadUnits = new List<Manifest.AssetInfo>();

        /// <summary>
        ///     All failed units
        /// </summary>
        private List<Manifest.AssetInfo> _failedUnits = new List<Manifest.AssetInfo>();

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
        ///     Total size need to download
        /// </summary>
        private ulong _totalSize;

        /// <summary>
        ///     Total number of assets to download
        /// </summary>
        private int _totalToDownload;

        /// <summary>
        ///     Total number of assets still waiting to be downloaded
        /// </summary>
        private int _totalWaitToDownload;

        private UpdateState _updateState = UpdateState.UNCHECKED;

        public AssetsUpdater(string storagePath, string hotFixUrl)
        {
            _storagePath = storagePath.EndsWith("/") ? storagePath : storagePath + "/";
            _hotFixUrl = hotFixUrl.EndsWith("/") ? hotFixUrl : hotFixUrl + "/";

            if (!Directory.Exists(_storagePath))
                Directory.CreateDirectory(_storagePath);

            _cacheVersionPath = _storagePath + VERSION_FILENAME;
            _cacheManifestPath = _storagePath + MANIFEST_FILENAME;
            _tempManifestPath = _storagePath + TEMP_MANIFEST_FILENAME;

            InitManifest();

            _hashChecker = HashChecker.Create(_storagePath);
            _hashChecker.OnCheckStarted += OnCheckStarted;
            _hashChecker.OnCheckProgress += OnCheckProgress;
            _hashChecker.OnCheckFinished += OnCheckFinished;
        }

        public int HashNum
        {
            get { return _hashChecker.HashNum; }
        }

        public int HashTotal
        {
            get { return _hashChecker.HashTotal; }
        }

        public string GameVersion
        {
            get { return _localManifest.GameVersion.ToString(); }
        }

        public string AssetsVersion
        {
            get { return _localManifest.AssetsVersion.ToString(); }
        }

        public ulong TotalSize
        {
            get { return _totalSize; }
        }

        public event Action<UpdateEvent> OnUpdateEvent;
        public Action<ulong, Action> OnUpdateConfirm;

        public void Release()
        {
            if (_hashChecker)
                Object.Destroy(_hashChecker.gameObject);
        }

        /// <summary>
        ///     Check for update
        /// </summary>
        public void CheckUpdate()
        {
            if (!_localManifest.Loaded)
            {
                Logger.Error(AssetsUpdaterLogTag, "AssetsUpdater : No local manifest file found.");
                DispatchUpdateEvent(UpdateEvent.EventCode.ERROR_NO_LOCAL_MANIFEST);
                return;
            }

            switch (_updateState)
            {
                case UpdateState.UNCHECKED:
                    DownloadVersion();
                    break;
                case UpdateState.UP_TO_DATE:
                    DispatchUpdateEvent(UpdateEvent.EventCode.ALREADY_UP_TO_DATE);
                    break;
                case UpdateState.NEED_UPDATE:
                case UpdateState.FAIL_TO_UPDATE:
                    DispatchUpdateEvent(UpdateEvent.EventCode.NEW_ASSETS_VERSION_FOUND);
                    break;
                case UpdateState.NEED_FORCE_UPDATE:
                    DispatchUpdateEvent(UpdateEvent.EventCode.NEW_GAME_VERSION_FOUND);
                    break;
            }
        }

        /// <summary>
        ///     Start update from remote, must call CheckUpdate first
        /// </summary>
        public void StartUpdate()
        {
            if (!_localManifest.Loaded)
            {
                Logger.Error(AssetsUpdaterLogTag, "AssetsUpdater : No local manifest file found.");
                DispatchUpdateEvent(UpdateEvent.EventCode.ERROR_NO_LOCAL_MANIFEST);
                return;
            }

            switch (_updateState)
            {
                case UpdateState.NEED_UPDATE:
                    DownloadManifest();
                    break;
                case UpdateState.FAIL_TO_UPDATE:
                    DownloadFailedAssets();
                    break;
            }
        }

        #region private methods

        private void InitManifest()
        {
            // local
            LoadLocalManifest();

            // temp
            if (File.Exists(_tempManifestPath))
            {
                _tempManifest.Parse(_tempManifestPath);
                if (!_tempManifest.Loaded)
                    File.Delete(_tempManifestPath);
            }
        }

        private void LoadLocalManifest()
        {
            // Find the cached manifest file
            Manifest cachedManifest = null;
            if (File.Exists(_cacheManifestPath))
            {
                Logger.Info(AssetsUpdaterLogTag, "Cache manifest found at path: {0}, parsing..", _cacheManifestPath);

                cachedManifest = new Manifest();
                cachedManifest.Parse(_cacheManifestPath);
                if (!cachedManifest.Loaded)
                {
                    File.Delete(_cacheManifestPath);
                    cachedManifest = null;
                }
            }

            // Load local manifest in app package
            Logger.Info(AssetsUpdaterLogTag, "Load local manifest at streaming assets: {0}, parsing..", MANIFEST_FILENAME);

            _localManifest.Parse(Path.Combine(Application.streamingAssetsPath, MANIFEST_FILENAME));
            if (_localManifest.Loaded)
                if (cachedManifest != null)
                {
                    var gvc = _localManifest.GameVersionCompareTo(cachedManifest);
                    var avc = _localManifest.AssetsVersionCompareTo(cachedManifest);

                    if (gvc != 0 || avc > 0)
                    {
                        Logger.Info(AssetsUpdaterLogTag,
                            "Local version({0}) greater than cache version({1}), deleting storage path: {2}..",
                            _localManifest.GameVersion, cachedManifest.GameVersion, _storagePath);

                        Directory.Delete(_storagePath, true);
                        Directory.CreateDirectory(_storagePath);
                    }
                    else
                    {
                        Logger.Info(AssetsUpdaterLogTag,
                            "Cache version({0}) greater than local version({1}), switching to cache manifest..",
                            cachedManifest.GameVersion, _localManifest.GameVersion);

                        _localManifest = cachedManifest;
                    }
                }
        }

        private void DownloadVersion()
        {
            var url = _hotFixUrl + VERSION_FILENAME;

            Logger.Info(AssetsUpdaterLogTag, "Start to download version file: {0}, to path: {1}", url, _cacheVersionPath);

            var task = DownloadManager.Instance.AddDownload(_cacheVersionPath, url);
            task.DownloadSuccess += args =>
            {
                Logger.Info(AssetsUpdaterLogTag, "Download version file succeed, parsing remote version..");
                ParseVersion();
            };
            task.DownloadFailure += args =>
            {
                Logger.Error(AssetsUpdaterLogTag, "AssetsUpdater : Fail to download version. " + args.Error);
                DispatchUpdateEvent(UpdateEvent.EventCode.ERROR_DOWNLOAD_VERSION);
                _updateState = UpdateState.UNCHECKED;
            };

            _updateState = UpdateState.DOWNLOADING_VERSION;
        }

        private void ParseVersion()
        {
            _remoteManifest.ParseVersion(_cacheVersionPath);
            if (!_remoteManifest.VersionLoaded)
            {
                Logger.Error(AssetsUpdaterLogTag, "AssetsUpdater : Error parsing version.");

                _updateState = UpdateState.UNCHECKED;
                DispatchUpdateEvent(UpdateEvent.EventCode.ERROR_PARSE_VERSION);
            }
            else
            {
                var gvc = _localManifest.GameVersionCompareTo(_remoteManifest);
                if (gvc == 0)
                {
                    var avc = _localManifest.AssetsVersionCompareTo(_remoteManifest);
                    if (avc >= 0)
                    {
                        _updateState = UpdateState.UP_TO_DATE;
                        DispatchUpdateEvent(UpdateEvent.EventCode.ALREADY_UP_TO_DATE);
                    }
                    else
                    {
                        _updateState = UpdateState.NEED_UPDATE;
                        DispatchUpdateEvent(UpdateEvent.EventCode.NEW_ASSETS_VERSION_FOUND);
                    }
                }
                else if (gvc < 0)
                {
                    _updateState = UpdateState.NEED_FORCE_UPDATE;
                    DispatchUpdateEvent(UpdateEvent.EventCode.NEW_GAME_VERSION_FOUND);
                }
                else
                {
                    Logger.Info(AssetsUpdaterLogTag, "Local Game Version({0}) > Remote Game Version({1})",
                        _localManifest.GameVersion, _remoteManifest.GameVersion);

                    _updateState = UpdateState.UP_TO_DATE;
                    DispatchUpdateEvent(UpdateEvent.EventCode.ALREADY_UP_TO_DATE);
                }
            }
        }

        private void DownloadManifest()
        {
            var url = _hotFixUrl + MANIFEST_FILENAME;

            Logger.Info(AssetsUpdaterLogTag, "Start to download manifest file: {0}, to path: {1}", url, _tempManifestPath);

            var task = DownloadManager.Instance.AddDownload(_tempManifestPath, url);
            task.DownloadSuccess += args =>
            {
                Logger.Info(AssetsUpdaterLogTag, "Download manifest file succeed, parsing remote manifest..");
                ParseManifest();
            };
            task.DownloadFailure += args =>
            {
                Logger.Error(AssetsUpdaterLogTag, "AssetsUpdater : Fail to download manifest." + args.Error);
                DispatchUpdateEvent(UpdateEvent.EventCode.ERROR_DOWNLOAD_MANIFEST);
                _updateState = UpdateState.NEED_UPDATE;
            };

            _updateState = UpdateState.DOWNLOADING_MANIFEST;
        }

        private void ParseManifest()
        {
            _remoteManifest.Parse(_tempManifestPath);

            if (!_remoteManifest.Loaded)
            {
                Logger.Error(AssetsUpdaterLogTag, "AssetsUpdater : Error parsing manifest.");
                DispatchUpdateEvent(UpdateEvent.EventCode.ERROR_PARSE_MANIFEST);
                _updateState = UpdateState.NEED_UPDATE;
            }
            else
            {
                DoUpdate();
            }
        }

        private void DoUpdate()
        {
            _updateState = UpdateState.UPDATING;

            _downloadUnits.Clear();
            _failedUnits.Clear();

            _totalToDownload = 0;
            _totalWaitToDownload = 0;
            _totalSize = 0;
            _downloadedSize.Clear();

            DownloadManager.Instance.RemoveAllDownloads();

            // Temporary manifest exists, resuming previous download
            if (_tempManifest.Loaded &&
                _tempManifest.GameVersionCompareTo(_remoteManifest) == 0 &&
                _tempManifest.AssetsVersionCompareTo(_remoteManifest) == 0)
            {
                _remoteManifest = _tempManifest;
                _downloadUnits = _remoteManifest.GenResumeAssetsList();
                _totalWaitToDownload = _totalToDownload = _downloadUnits.Count;
                if (_totalWaitToDownload <= 0)
                {
                    OnDownloadUnitsFinished();
                    return;
                }

                _totalSize = CalculateTotalSize(_downloadUnits);
                _remoteManifest.SaveToFile(_tempManifestPath);

                var msg =
                    string.Format(
                        "Resuming from previous update, {0} assets remains to update.",
                        _totalToDownload);
                Logger.Info(AssetsUpdaterLogTag, msg);
                DispatchUpdateEvent(UpdateEvent.EventCode.UPDATE_PROGRESSION);

                if (OnUpdateConfirm != null)
                {
                    OnUpdateConfirm(_totalSize, BatchDownload);
                }
                else
                {
                    BatchDownload();
                }
            }
            else
            {
                // Temporary manifest not exists or out of date,
                var diffDic = _localManifest.GenDiff(_remoteManifest);
                if (diffDic.Count == 0)
                {
                    UpdateSucceed();
                }
                else
                {
                    foreach (var diffKV in diffDic)
                    {
                        var diff = diffKV.Value;
                        if (diff.diffType == Manifest.DiffType.DELETED)
                        {
                            if (File.Exists(_storagePath + diff.asset.fileName))
                                File.Delete(_storagePath + diff.asset.fileName);
                        }
                        else
                        {
                            _downloadUnits.Add(diff.asset);
                        }
                    }

                    var assets = _remoteManifest.GetAssets();
                    foreach (var assetKV in assets)
                    {
                        var assetName = assetKV.Key;
                        if (!diffDic.ContainsKey(assetName))
                            _remoteManifest.SetAssetDownloadState(assetName, Manifest.DownloadState.SUCCEED);
                    }
                    _remoteManifest.SaveToFile(_tempManifestPath);

                    _totalWaitToDownload = _totalToDownload = _downloadUnits.Count;
                    _totalSize = CalculateTotalSize(_downloadUnits);

                    var msg = string.Format("Start to update {0} assets.", _totalToDownload);
                    Logger.Info(AssetsUpdaterLogTag, msg);
                    DispatchUpdateEvent(UpdateEvent.EventCode.UPDATE_PROGRESSION);

                    if (OnUpdateConfirm != null)
                    {
                        OnUpdateConfirm(_totalSize, BatchDownload);
                    }
                    else
                    {
                        BatchDownload();
                    }
                }
            }
        }

        private ulong CalculateTotalSize(List<Manifest.AssetInfo> assets)
        {
            ulong size = 0;
            foreach (var asset in assets) size += asset.size;

            return size > 0 ? size : 1;
        }

        private void DownloadFailedAssets()
        {
            if (_failedUnits.Count == 0) return;

            _downloadedSize.Clear();

            _updateState = UpdateState.UPDATING;
            _downloadUnits.Clear();
            _downloadUnits = _failedUnits;
            _failedUnits = new List<Manifest.AssetInfo>();
            _totalWaitToDownload = _totalToDownload = _downloadUnits.Count;
            _totalSize = CalculateTotalSize(_downloadUnits);

            var msg = string.Format("Start to update {0} failed assets.", _totalWaitToDownload);
            Logger.Info(AssetsUpdaterLogTag, msg);
            DispatchUpdateEvent(UpdateEvent.EventCode.UPDATE_PROGRESSION);

            BatchDownload();
        }

        private void BatchDownload()
        {
            if (_downloadUnits.Count <= 0)
            {
                OnDownloadUnitsFinished();
                return;
            }

            foreach (var asset in _downloadUnits)
            {
                var storagePath = _storagePath + asset.fileName;
                var url = _hotFixUrl + asset.fileName;

                var dir = Path.GetDirectoryName(storagePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var task = DownloadManager.Instance.AddDownload(storagePath, url, asset);
                task.DownloadSuccess += OnDownloadSuccess;
                task.DownloadFailure += OnDownloadError;
                task.DownloadUpdate += OnDownloadProgress;
            }
        }

        private void OnDownloadUnitsFinished()
        {
            Logger.Info(AssetsUpdaterLogTag, "Download Finish - {0} download failed.", _failedUnits.Count);

            // Release file locks.
            DownloadManager.Instance.RemoveAllDownloads();

            if (_failedUnits.Count > 0)
            {
                UpdateFailed();
            }
            else
            {
                var assets = _remoteManifest.GetDownloadedAssets();
                _hashChecker.Check(assets);
            }
        }

        private void UpdateFailed()
        {
            _remoteManifest.SaveToFile(_tempManifestPath);
            _updateState = UpdateState.FAIL_TO_UPDATE;
            DispatchUpdateEvent(UpdateEvent.EventCode.UPDATE_FAILED);
        }

        private void UpdateSucceed()
        {
            _localManifest = _remoteManifest;
            _remoteManifest = null;

            // rename temporary manifest to valid manifest
            if (File.Exists(_cacheManifestPath))
                File.Delete(_cacheManifestPath);

            File.Move(_tempManifestPath, _cacheManifestPath);

            _updateState = UpdateState.UP_TO_DATE;
            DispatchUpdateEvent(UpdateEvent.EventCode.UPDATE_FINISHED);
        }

        private void OnDownloadSuccess(DownloadEventArgs args)
        {
            var asset = (Manifest.AssetInfo) args.UserData;

            _remoteManifest.SetAssetDownloadState(asset.fileName, Manifest.DownloadState.DOWNLOADED);
            _remoteManifest.SaveToFile(_tempManifestPath);

            _totalWaitToDownload--;
            RecordDownloadedSize(asset.fileName, asset.size);
            DispatchUpdateEvent(UpdateEvent.EventCode.UPDATE_PROGRESSION);
            DispatchUpdateEvent(UpdateEvent.EventCode.ASSET_UPDATED, asset.fileName);

            if (_totalWaitToDownload <= 0)
                OnDownloadUnitsFinished();
        }

        private void OnDownloadProgress(DownloadEventArgs args)
        {
            var asset = (Manifest.AssetInfo) args.UserData;
            RecordDownloadedSize(asset.fileName, args.DownloadedSize);

            if (Time.realtimeSinceStartup - _lastUpdateTime < UPDATE_PROGRESS_INTERVAL) return;

            DispatchUpdateEvent(UpdateEvent.EventCode.UPDATE_PROGRESSION);
            _lastUpdateTime = Time.realtimeSinceStartup;
        }

        private void RecordDownloadedSize(string assetName, ulong size)
        {
            if (_downloadedSize.ContainsKey(assetName))
                _downloadedSize[assetName] = size;
            else
                _downloadedSize.Add(assetName, size);
        }

        private void OnDownloadError(DownloadEventArgs args)
        {
            var asset = (Manifest.AssetInfo) args.UserData;
            Logger.Error(AssetsUpdaterLogTag, asset.fileName + ": download failed!" + args.Error);

            _totalWaitToDownload--;
            _failedUnits.Add(asset);
            DispatchUpdateEvent(UpdateEvent.EventCode.ERROR_UPDATING, asset.fileName);

            if (_totalWaitToDownload <= 0) OnDownloadUnitsFinished();
        }

        private void DispatchUpdateEvent(UpdateEvent.EventCode code, string assetName = "")
        {
            if (OnUpdateEvent == null) return;

            var evt = new UpdateEvent {Code = code, AssetName = assetName};

            if (code == UpdateEvent.EventCode.UPDATE_PROGRESSION)
            {
                evt.DownloadedSize = CalculateDownloadedSize();
                evt.Percent = (float) evt.DownloadedSize / _totalSize;
                evt.PercentByFile = (float) (_totalToDownload - _totalWaitToDownload - _failedUnits.Count) /
                                    _totalToDownload;
                evt.DownloadSpeed = DownloadManager.Instance.FormatedSpeed;
            }

            if (OnUpdateEvent != null)
                OnUpdateEvent(evt);
        }

        private ulong CalculateDownloadedSize()
        {
            ulong size = 0;
            foreach (var kv in _downloadedSize) size += kv.Value;

            return size;
        }

        private void OnCheckStarted()
        {
            DispatchUpdateEvent(UpdateEvent.EventCode.HASH_START);
        }

        private void OnCheckProgress(Manifest.AssetInfo asset, bool valid)
        {
            if (valid)
            {
                _remoteManifest.SetAssetDownloadState(asset.fileName, Manifest.DownloadState.SUCCEED);
            }
            else
            {
                _remoteManifest.SetAssetDownloadState(asset.fileName, Manifest.DownloadState.UNSTARTED);
                _failedUnits.Add(asset);

                Logger.Error(AssetsUpdaterLogTag, "Hash Invalid : {0}", asset.fileName);
            }
            _remoteManifest.SaveToFile(_tempManifestPath);

            DispatchUpdateEvent(UpdateEvent.EventCode.HASH_PROGRESSION);
        }

        private void OnCheckFinished()
        {
            if (_hashChecker.Valid)
                UpdateSucceed();
            else
                UpdateFailed();
        }

        #endregion
    }
}