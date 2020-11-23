using vFrame.Core.FileSystems.Constants;
using vFrame.Core.FileSystems.Unity.Adapters;
using vFrame.Core.FileSystems.Unity.StreamingAssets;
using vFrame.Core.Utils;

namespace vFrame.Core.FileSystems.Unity
{
    public class FileSystemManager : FileSystemManager<UnityFileStreamFactory>
    {
        protected override void OnCreate() {
            BetterStreamingAssets.Initialize();
            PathUtils.Initialize();

            base.OnCreate(new UnityFileStreamFactory());
        }

        public new IVirtualFileSystem AddFileSystem(VFSPath vfsPath) {
            IVirtualFileSystem virtualFileSystem;

            switch (vfsPath.GetExtension()) {
                case PackageFileSystemConst.Ext:
                    return base.AddFileSystem(vfsPath);
                default:
                    if (!PathUtils.IsStreamingAssetsPath(vfsPath)) {
                        return base.AddFileSystem(vfsPath);
                    }
                    virtualFileSystem = new SAStandardVirtualFileSystem(_factory);
                    break;
            }

            virtualFileSystem.Open(vfsPath);

            AddFileSystem(virtualFileSystem);
            return virtualFileSystem;
        }
    }
}