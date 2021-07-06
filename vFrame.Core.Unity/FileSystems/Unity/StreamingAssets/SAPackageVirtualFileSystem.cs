﻿using System;
using vFrame.Core.FileSystems.Package;
using vFrame.Core.Utils;

namespace vFrame.Core.FileSystems.Unity.StreamingAssets
{
    public class SAPackageVirtualFileSystem : PackageVirtualFileSystem
    {
        public override void Open(VFSPath fsPath) {
            if (!PathUtils.IsStreamingAssetsPath(fsPath)) {
                throw new ArgumentException("Input argument must be streaming-assets path.");
            }

            var relativePath = PathUtils.AbsolutePathToRelativeStreamingAssetsPath(fsPath);
            var stream = BetterStreamingAssets.OpenRead(relativePath);

            Open(stream);

            ReadOnly = true;
            PackageFilePath = fsPath;
        }
    }
}