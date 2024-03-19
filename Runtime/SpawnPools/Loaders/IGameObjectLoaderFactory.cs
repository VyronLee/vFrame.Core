namespace vFrame.Core.Unity.SpawnPools
{
    public interface IGameObjectLoaderFactory
    {
        IGameObjectLoader CreateLoader(string assetPath);
    }
}