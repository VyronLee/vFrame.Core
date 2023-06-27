//------------------------------------------------------------
//        File:  PathUtils.cs
//       Brief:  PathUtils
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-02-15 20:15
//   Copyright:  Copyright (c) 2018, VyronLee
//============================================================

using System.IO;
using UnityEngine;

namespace vFrame.Core.Utils
{
    public static class PathUtils
    {
        public static string NormalizePath(this string value) {
            value = value.Replace("\\", "/");
            return value;
        }
        
        public static string NormalizeLocalFileURL(this string value) {
#if UNITY_ANDROID || UNITY_WEBGL
            return NormalizePath(value);
#else
            var url = NormalizePath(value);
            if (!url.StartsWith("file://")) {
                url = "file://" + url;
            }
            return url;
#endif
        }

        public static string NormalizeDirectoryPath(this string value) {
            var path = NormalizePath(value);
            if (!path.EndsWith("/")) {
                path += "/";
            }
            return path;
        }

        public static void EnsureFileWritePath(this string path) {
            var dirName = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(dirName)) {
                return;
            }
            if (Directory.Exists(dirName)) {
                return;
            }
            Directory.CreateDirectory(dirName);
        }

        public static string Combine(string path1, string path2) {
            var value = Path.Combine(path1, path2);
            return NormalizePath(value);
        }

        public static string Combine(params string[] paths) {
            if (paths == null || paths.Length == 0) {
                return "";
            }

            if (paths.Length <= 1) {
                return paths[0];
            }

            var path = paths[0];
            for (var i = 1; i < paths.Length; i++) {
                path = Path.Combine(path, paths[i]);
            }
            return NormalizePath(path);
        }

        public static string NormalizeAssetBundlePath(this string value) {
            if (Path.IsPathRooted(value))
                value = AbsolutePathToRelativeDataPath(value);

            value = value.ToLower();
            return NormalizePath(value);
        }

        public static string AbsolutePathToRelativeProjectPath(this string fullPath) {
            var path = AbsolutePathToRelativeDataPath(fullPath);
            path = Path.Combine("Assets", path);
            return NormalizePath(path);
        }

        public static string AbsolutePathToRelativeDataPath(this string fullPath) {
            fullPath = NormalizePath(fullPath);
            var projDataFullPath = NormalizePath(Path.GetFullPath(DataPath) + "/");
            var relativaPath = fullPath.Replace(projDataFullPath, "");
            return NormalizePath(relativaPath);
        }

        public static string AbsolutePathToRelativeResourcesPath(this string fullPath) {
            fullPath = NormalizePath(fullPath);
            var resourcesPath = Path.Combine(DataPath, "Resources");
            var projDataFullPath = NormalizePath(resourcesPath + "/");
            var relativePath = fullPath.Replace(projDataFullPath, "");
            return NormalizePath(relativePath);
        }

        public static string AbsolutePathToRelativeStreamingAssetsPath(this string fullPath) {
            //Debug.LogError("AbsolutePathToRelativeStreamingAssetsPath - fullPath: " + fullPath);
            fullPath = NormalizePath(fullPath);
            var projDataFullPath = NormalizePath(StreamingAssetsPath + "/");
            var relativePath = fullPath.Replace(projDataFullPath, "");
            return NormalizePath(relativePath);
        }

        public static string RelativeDataPathToAbsolutePath(this string relativePath) {
            return Combine(DataPath, relativePath);
        }

        public static string RelativeProjectPathToAbsolutePath(this string relativePath) {
            var dataPath = DataPath;
            var projectPath = dataPath.Remove(dataPath.Length - 6, 6);
            return Combine(projectPath, relativePath);
        }

        public static string RelativeResourcesPathToAbsolutePath(this string relativePath) {
            var resourcesPath = Path.Combine(DataPath, "Resources");
            return Combine(resourcesPath, relativePath);
        }

        public static string RelativeStreamingAssetsPathToAbsolutePath(this string relativePath) {
            return Combine(StreamingAssetsPath, relativePath);
        }

        public static string RelativeProjectPathToRelativeDataPath(this string relativePath) {
            return relativePath.Remove(0, 7);
        }

        public static string RelativeProjectPathToRelativeResourcesPath(this string relativePath) {
            return relativePath.Remove(0, 17);
        }

        public static string RelativeDataPathToRelativeProjectPath(this string relativePath) {
            return string.Format("Assets/{0}", relativePath);
        }

        public static string RelativeDataPathToRelativeResourcesPath(this string relativePath) {
            return relativePath.Remove(0, 10);
        }

        public static string RelativeResourcesPathToRelativeDataPath(this string relativePath) {
            return string.Format("Resources/{0}", relativePath);
        }

        public static string RelativeResourcesPathToRelativeProjectPath(this string relativePath) {
            return string.Format("Assets/Resources/{0}", relativePath);
        }

        public static string GetBundleName(string value) {
            value = Path.Combine(
                Path.GetDirectoryName(value),
                Path.GetFileNameWithoutExtension(value));
            value = NormalizeAssetBundlePath(value);
            return value;
        }

        public static string GetAssetName(string value) {
            value = Path.GetFileNameWithoutExtension(value);
            return value;
        }

        public static bool IsFileInPersistentDataPath(this string path) {
            path = NormalizePath(path);
            return path.StartsWith(PersistentDataPath);
        }

        public static bool IsStreamingAssetsPath(this string path) {
            path = NormalizePath(path);
            return path.StartsWith(StreamingAssetsPath);
        }

        private static string _dataPath;
        private static string DataPath {
            get {
                if (string.IsNullOrEmpty(_dataPath)) {
                    _dataPath = Application.dataPath;
                }
                return _dataPath;
            }
        }

        private static string _streamingAssetsPath;
        private static string StreamingAssetsPath {
            get {
                if (string.IsNullOrEmpty(_streamingAssetsPath)) {
                    _streamingAssetsPath = Application.streamingAssetsPath;
                }
                return _streamingAssetsPath;
            }
        }

        private static string _persistentDataPath;
        private static string PersistentDataPath {
            get {
                if (string.IsNullOrEmpty(_persistentDataPath)) {
                    _persistentDataPath = Application.persistentDataPath;
                }
                return _persistentDataPath;
            }
        }

        public static void Initialize() {
            var _ = "";
            _ = DataPath;
            _ = StreamingAssetsPath;
            _ = PersistentDataPath;
        }
    }
}