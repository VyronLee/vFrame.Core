// ------------------------------------------------------------
//         File: PathUtils.cs
//        Brief: Path manipulation utilities for Unity project and streaming assets paths.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-02-15 20:15:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System.IO;
using UnityEngine;

namespace vFrame.Core.Unity
{
    public static class PathUtils
    {
        private static string _dataPath;

        private static string _streamingAssetsPath;

        private static string _persistentDataPath;

        /// <summary>
        ///     Gets the cached <see cref="Application.dataPath"/> value.
        /// </summary>
        private static string DataPath {
            get {
                if (string.IsNullOrEmpty(_dataPath)) {
                    _dataPath = Application.dataPath;
                }
                return _dataPath;
            }
        }

        /// <summary>
        ///     Gets the cached <see cref="Application.streamingAssetsPath"/> value.
        /// </summary>
        private static string StreamingAssetsPath {
            get {
                if (string.IsNullOrEmpty(_streamingAssetsPath)) {
                    _streamingAssetsPath = Application.streamingAssetsPath;
                }
                return _streamingAssetsPath;
            }
        }

        /// <summary>
        ///     Gets the cached <see cref="Application.persistentDataPath"/> value.
        /// </summary>
        private static string PersistentDataPath {
            get {
                if (string.IsNullOrEmpty(_persistentDataPath)) {
                    _persistentDataPath = Application.persistentDataPath;
                }
                return _persistentDataPath;
            }
        }

        /// <summary>
        ///     Normalizes a path string by replacing backslashes with forward slashes.
        /// </summary>
        /// <param name="value">The path string to normalize.</param>
        /// <returns>The normalized path using forward slashes.</returns>
        public static string NormalizePath(this string value) {
            value = value.Replace("\\", "/");
            return value;
        }

        /// <summary>
        ///     Converts a local file path to a proper file URL.
        ///     On Android and WebGL the path is returned as-is after normalization.
        ///     On other platforms a <c>file://</c> prefix is added if missing.
        /// </summary>
        /// <param name="value">The local file path to convert.</param>
        /// <returns>A normalized file URL string.</returns>
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

        /// <summary>
        ///     Normalizes a directory path by ensuring it ends with a trailing slash.
        /// </summary>
        /// <param name="value">The directory path to normalize.</param>
        /// <returns>The normalized directory path with a trailing forward slash.</returns>
        public static string NormalizeDirectoryPath(this string value) {
            var path = NormalizePath(value);
            if (!path.EndsWith("/")) {
                path += "/";
            }
            return path;
        }

        /// <summary>
        ///     Ensures the directory portion of a file path exists,
        ///     creating it recursively if necessary.
        /// </summary>
        /// <param name="path">The file path whose parent directory should exist.</param>
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

        /// <summary>
        ///     Combines two path segments and normalizes the result to use forward slashes.
        /// </summary>
        /// <param name="path1">The first path segment.</param>
        /// <param name="path2">The second path segment.</param>
        /// <returns>The combined and normalized path.</returns>
        public static string Combine(string path1, string path2) {
            var value = Path.Combine(path1, path2);
            return NormalizePath(value);
        }

        /// <summary>
        ///     Combines multiple path segments and normalizes the result to use forward slashes.
        /// </summary>
        /// <param name="paths">An array of path segments to combine.</param>
        /// <returns>
        ///     The combined and normalized path, or an empty string if
        ///     <paramref name="paths"/> is null or empty.
        /// </returns>
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

        /// <summary>
        ///     Normalizes a path for use as an AssetBundle name.
        ///     Rooted paths are converted to data-path-relative paths first,
        ///     then lowercased and normalized to forward slashes.
        /// </summary>
        /// <param name="value">The path to normalize.</param>
        /// <returns>The normalized, lowercased AssetBundle path.</returns>
        public static string NormalizeAssetBundlePath(this string value) {
            if (Path.IsPathRooted(value)) {
                value = AbsolutePathToRelativeDataPath(value);
            }

            value = value.ToLower();
            return NormalizePath(value);
        }

        /// <summary>
        ///     Converts an absolute path to a project-relative path prefixed with <c>Assets/</c>.
        /// </summary>
        /// <param name="fullPath">The absolute path to convert.</param>
        /// <returns>A project-relative path starting with <c>Assets/</c>.</returns>
        public static string AbsolutePathToRelativeProjectPath(this string fullPath) {
            var path = AbsolutePathToRelativeDataPath(fullPath);
            path = Path.Combine("Assets", path);
            return NormalizePath(path);
        }

        /// <summary>
        ///     Converts an absolute path to a path relative to <see cref="Application.dataPath"/>.
        /// </summary>
        /// <param name="fullPath">The absolute path to convert.</param>
        /// <returns>A path relative to the data path.</returns>
        public static string AbsolutePathToRelativeDataPath(this string fullPath) {
            fullPath = NormalizePath(fullPath);
            var projDataFullPath = NormalizePath(Path.GetFullPath(DataPath) + "/");
            var relativaPath = fullPath.Replace(projDataFullPath, "");
            return NormalizePath(relativaPath);
        }

        /// <summary>
        ///     Converts an absolute path to a path relative to the <c>Resources</c> folder
        ///     inside <see cref="Application.dataPath"/>.
        /// </summary>
        /// <param name="fullPath">The absolute path to convert.</param>
        /// <returns>A path relative to the Resources folder.</returns>
        public static string AbsolutePathToRelativeResourcesPath(this string fullPath) {
            fullPath = NormalizePath(fullPath);
            var resourcesPath = Path.Combine(DataPath, "Resources");
            var projDataFullPath = NormalizePath(resourcesPath + "/");
            var relativePath = fullPath.Replace(projDataFullPath, "");
            return NormalizePath(relativePath);
        }

        /// <summary>
        ///     Converts an absolute path to a path relative to
        ///     <see cref="Application.streamingAssetsPath"/>.
        /// </summary>
        /// <param name="fullPath">The absolute path to convert.</param>
        /// <returns>A path relative to the streaming assets folder.</returns>
        public static string AbsolutePathToRelativeStreamingAssetsPath(this string fullPath) {
            fullPath = NormalizePath(fullPath);
            var projDataFullPath = NormalizePath(StreamingAssetsPath + "/");
            var relativePath = fullPath.Replace(projDataFullPath, "");
            return NormalizePath(relativePath);
        }

        /// <summary>
        ///     Converts a data-path-relative path to an absolute path.
        /// </summary>
        /// <param name="relativePath">The path relative to <see cref="Application.dataPath"/>.</param>
        /// <returns>The absolute path.</returns>
        public static string RelativeDataPathToAbsolutePath(this string relativePath) {
            return Combine(DataPath, relativePath);
        }

        /// <summary>
        ///     Converts a project-relative path (e.g. <c>Assets/Foo.bar</c>) to an absolute path.
        /// </summary>
        /// <param name="relativePath">The project-relative path.</param>
        /// <returns>The absolute path on disk.</returns>
        public static string RelativeProjectPathToAbsolutePath(this string relativePath) {
            var dataPath = DataPath;
            var projectPath = dataPath.Remove(dataPath.Length - 6, 6);
            return Combine(projectPath, relativePath);
        }

        /// <summary>
        ///     Converts a Resources-folder-relative path to an absolute path.
        /// </summary>
        /// <param name="relativePath">The path relative to the Resources folder.</param>
        /// <returns>The absolute path.</returns>
        public static string RelativeResourcesPathToAbsolutePath(this string relativePath) {
            var resourcesPath = Path.Combine(DataPath, "Resources");
            return Combine(resourcesPath, relativePath);
        }

        /// <summary>
        ///     Converts a streaming-assets-relative path to an absolute path.
        /// </summary>
        /// <param name="relativePath">The path relative to the streaming assets folder.</param>
        /// <returns>The absolute path.</returns>
        public static string RelativeStreamingAssetsPathToAbsolutePath(this string relativePath) {
            return Combine(StreamingAssetsPath, relativePath);
        }

        /// <summary>
        ///     Converts a project-relative path (e.g. <c>Assets/Foo.bar</c>) to a
        ///     data-path-relative path by removing the leading <c>Assets/</c> prefix.
        /// </summary>
        /// <param name="relativePath">The project-relative path.</param>
        /// <returns>A path relative to <see cref="Application.dataPath"/>.</returns>
        public static string RelativeProjectPathToRelativeDataPath(this string relativePath) {
            return relativePath.Remove(0, 7);
        }

        /// <summary>
        ///     Converts a project-relative path to a Resources-relative path
        ///     by removing the leading <c>Assets/Resources/</c> prefix.
        /// </summary>
        /// <param name="relativePath">The project-relative path.</param>
        /// <returns>A path relative to the Resources folder.</returns>
        public static string RelativeProjectPathToRelativeResourcesPath(this string relativePath) {
            return relativePath.Remove(0, 17);
        }

        /// <summary>
        ///     Converts a data-path-relative path to a project-relative path
        ///     by prepending <c>Assets/</c>.
        /// </summary>
        /// <param name="relativePath">The data-path-relative path.</param>
        /// <returns>A project-relative path starting with <c>Assets/</c>.</returns>
        public static string RelativeDataPathToRelativeProjectPath(this string relativePath) {
            return string.Format("Assets/{0}", relativePath);
        }

        /// <summary>
        ///     Converts a data-path-relative path to a Resources-relative path
        ///     by removing the leading <c>Resources/</c> prefix.
        /// </summary>
        /// <param name="relativePath">The data-path-relative path.</param>
        /// <returns>A path relative to the Resources folder.</returns>
        public static string RelativeDataPathToRelativeResourcesPath(this string relativePath) {
            return relativePath.Remove(0, 10);
        }

        /// <summary>
        ///     Converts a Resources-relative path to a data-path-relative path
        ///     by prepending <c>Resources/</c>.
        /// </summary>
        /// <param name="relativePath">The Resources-relative path.</param>
        /// <returns>A path relative to <see cref="Application.dataPath"/>.</returns>
        public static string RelativeResourcesPathToRelativeDataPath(this string relativePath) {
            return string.Format("Resources/{0}", relativePath);
        }

        /// <summary>
        ///     Converts a Resources-relative path to a project-relative path
        ///     by prepending <c>Assets/Resources/</c>.
        /// </summary>
        /// <param name="relativePath">The Resources-relative path.</param>
        /// <returns>A project-relative path starting with <c>Assets/Resources/</c>.</returns>
        public static string RelativeResourcesPathToRelativeProjectPath(this string relativePath) {
            return string.Format("Assets/Resources/{0}", relativePath);
        }

        /// <summary>
        ///     Derives an AssetBundle name from the given path by removing the file extension
        ///     and normalizing the result.
        /// </summary>
        /// <param name="value">An asset path used to derive the bundle name.</param>
        /// <returns>The normalized, lowercased bundle name.</returns>
        public static string GetBundleName(string value) {
            value = Path.Combine(
                Path.GetDirectoryName(value),
                Path.GetFileNameWithoutExtension(value));
            value = NormalizeAssetBundlePath(value);
            return value;
        }

        /// <summary>
        ///     Extracts the asset name (file name without extension) from the given path.
        /// </summary>
        /// <param name="value">The path to extract the asset name from.</param>
        /// <returns>The file name without its extension.</returns>
        public static string GetAssetName(string value) {
            value = Path.GetFileNameWithoutExtension(value);
            return value;
        }

        /// <summary>
        ///     Determines whether the given path is located inside
        ///     <see cref="Application.persistentDataPath"/>.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns><c>true</c> if the path starts with the persistent data path; otherwise, <c>false</c>.</returns>
        public static bool IsFileInPersistentDataPath(this string path) {
            path = NormalizePath(path);
            return path.StartsWith(PersistentDataPath);
        }

        /// <summary>
        ///     Determines whether the given path is located inside
        ///     <see cref="Application.streamingAssetsPath"/>.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns><c>true</c> if the path starts with the streaming assets path; otherwise, <c>false</c>.</returns>
        public static bool IsStreamingAssetsPath(this string path) {
            path = NormalizePath(path);
            return path.StartsWith(StreamingAssetsPath);
        }

        /// <summary>
        ///     Pre-warms the cached path values so subsequent accesses avoid
        ///     querying <see cref="Application"/> properties on the main thread.
        /// </summary>
        public static void Initialize() {
            var _ = "";
            _ = DataPath;
            _ = StreamingAssetsPath;
            _ = PersistentDataPath;
        }
    }
}
