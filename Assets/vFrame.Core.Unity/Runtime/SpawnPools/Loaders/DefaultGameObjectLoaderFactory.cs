namespace vFrame.Core.Unity.SpawnPools
{
    internal class DefaultGameObjectLoaderFactory : IGameObjectLoaderFactory
    {
        public IGameObjectLoader CreateLoader(string assetPath) {
            var ret = new DefaultGameObjectLoader();
            ret.Create(assetPath);
            return ret;
        }
    }
}