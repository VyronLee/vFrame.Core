namespace vFrame.Core.SpawnPools.Builders
{
    internal class DefaultGameObjectBuilderFromPathFactory : IGameObjectBuilderFactory
    {
        public IGameObjectBuilder CreateBuilder() {
            return new DefaultGameObjectBuilderFromPath();
        }
    }
}