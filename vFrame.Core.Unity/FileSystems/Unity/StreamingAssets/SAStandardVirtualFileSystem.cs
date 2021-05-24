using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using vFrame.Core.Utils;

namespace vFrame.Core.FileSystems.Unity.StreamingAssets
{
    public class SAStandardVirtualFileSystem : VirtualFileSystem
    {
        private VFSPath _workingDir;

        public override void Open(VFSPath fsPath) {
            if (fsPath == Application.streamingAssetsPath) {
                // Root directory
            }
            else if (!BetterStreamingAssets.DirectoryExists(fsPath)) {
                throw new DirectoryNotFoundException();
            }

            if (!PathUtils.IsStreamingAssetsPath(fsPath)) {
                throw new ArgumentException("Input argument must be streaming-assets path.");
            }

            _workingDir = fsPath.AsDirectory();
            _workingDir = PathUtils.AbsolutePathToRelativeStreamingAssetsPath(_workingDir);
        }

        public override void Open(Stream stream) {
            throw new NotSupportedException();
        }

        public override void Close() {

        }

        public override bool Exist(VFSPath relativeVfsPath) {
            return BetterStreamingAssets.FileExists(_workingDir + relativeVfsPath);
        }

        public override IVirtualFileStream GetStream(VFSPath fileName, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read,
            FileShare share = FileShare.Read) {
            var fullPath = _workingDir + fileName;
            var stream = BetterStreamingAssets.OpenRead(fullPath);
            return new SAStandardVirtualFileStream(stream);
        }

        public override IReadonlyVirtualFileStreamRequest GetReadonlyStreamAsync(VFSPath fileName) {
            var fullPath = _workingDir + fileName;
            return new SAStandardReadonlyVirtualFileStreamRequest(fullPath);
        }

        public override IList<VFSPath> List(IList<VFSPath> refs) {
            throw new NotSupportedException();
        }

        public override event OnGetStreamEventHandler OnGetStream;

        public override string ToString() {
            return VFSPath.GetPath(Application.streamingAssetsPath) + _workingDir;
        }
    }
}
