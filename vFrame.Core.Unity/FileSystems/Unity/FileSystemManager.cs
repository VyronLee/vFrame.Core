using vFrame.Core.FileSystems.Unity.Adapters;

namespace vFrame.Core.FileSystems.Unity
{
    public class FileSystemManager : FileSystemManager<UnityFileStreamFactory>
    {
        protected override void OnCreate() {
            BetterStreamingAssets.Initialize();
            base.OnCreate(new UnityFileStreamFactory());
        }
    }
}