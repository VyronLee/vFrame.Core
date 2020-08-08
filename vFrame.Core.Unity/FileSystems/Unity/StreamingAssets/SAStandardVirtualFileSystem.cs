using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using vFrame.Core.FileSystems.Adapters;
using vFrame.Core.Utils;

namespace vFrame.Core.FileSystems.Unity.StreamingAssets
{
    public class SAStandardVirtualFileSystem : VirtualFileSystem
    {
        private VFSPath _workingDir;

        public SAStandardVirtualFileSystem() : this(null) {

        }

        public SAStandardVirtualFileSystem(FileStreamFactory factory) : base(factory) {

        }

        public override void Open(VFSPath streamVfsPath) {
            if (streamVfsPath == Application.streamingAssetsPath) {
                // Root directory
            }
            else if (!BetterStreamingAssets.DirectoryExists(streamVfsPath)) {
                throw new DirectoryNotFoundException();
            }

            _workingDir = streamVfsPath.AsDirectory();
            _workingDir = PathUtils.AbsolutePathToRelativeStreamingAssetsPath(_workingDir);
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
    }
}
