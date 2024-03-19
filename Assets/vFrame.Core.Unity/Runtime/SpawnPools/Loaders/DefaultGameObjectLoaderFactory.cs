namespace vFrame.Core.Unity.SpawnPools
{
    internal class DefaultGameObjectLoaderFactory : IGameObjectLoaderFactory
    {
        public IGameObjectLoader CreateLoader(string assetPath) {
            return DefaultGameObjectLoader.Create(assetPath);
        }
    }
}