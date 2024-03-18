namespace vFrame.Core.Unity.SpawnPools
{
    internal class DefaultGameObjectLoaderFromPathFactory : IGameObjectLoaderFactory
    {
        public IGameObjectLoader CreateLoader() {
            return new DefaultGameObjectLoaderFromPath();
        }
    }
}