namespace vFrame.Core.SpawnPools.Snapshots.Impls
{
    public class GameObjectSnapshot : Snapshot
    {
        private int _layer;
        private string _tag;

        public override void Take() {
            _layer = Target.layer;
            _tag = Target.tag;
        }

        public override void Restore() {
            Target.layer = _layer;
            Target.tag = _tag;
        }
    }
}