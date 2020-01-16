namespace vFrame.Core.SpawnPools.Builders
{
    internal class DefaultGameObjectBuilderFromPrefabInstanceFactory : IGameObjectBuilderFactory
    {
        public IGameObjectBuilder CreateBuilder() {
            return new DefaultGameObjectBuilderFromPrefabInstance();
        }
    }
}