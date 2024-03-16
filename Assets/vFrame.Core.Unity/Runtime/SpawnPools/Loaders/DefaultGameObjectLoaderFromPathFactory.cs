namespace vFrame.Core.SpawnPools.Loaders
{
    internal class DefaultGameObjectLoaderFromPathFactory : IGameObjectLoaderFactory
    {
        public IGameObjectLoader CreateLoader() {
            return new DefaultGameObjectLoaderFromPath();
        }
    }
}