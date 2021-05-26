using vFrame.Core.FileSystems.Constants;
using vFrame.Core.FileSystems.Unity.StreamingAssets;
using vFrame.Core.Utils;

namespace vFrame.Core.FileSystems.Unity
{
    public class FileSystemManager : vFrame.Core.FileSystems.FileSystemManager
    {
        protected override void OnCreate() {
            BetterStreamingAssets.Initialize();
            PathUtils.Initialize();
            base.OnCreate();
        }

        public new IVirtualFileSystem AddFileSystem(VFSPath vfsPath) {
            IVirtualFileSystem virtualFileSystem;

            if (!PathUtils.IsStreamingAssetsPath(vfsPath)) {
                return base.AddFileSystem(vfsPath);
            }

            switch (vfsPath.GetExtension().ToLower()) {
                case PackageFileSystemConst.Ext:
                    virtualFileSystem = new SAPackageVirtualFileSystem();
                    break;
                default:
                    virtualFileSystem = new SAStandardVirtualFileSystem();
                    break;
            }

            virtualFileSystem.Open(vfsPath);

            AddFileSystem(virtualFileSystem);
            return virtualFileSystem;
        }
    }
}