using UnityEngine;
using vFrame.Core.Base;

namespace vFrame.Core.SpawnPools.Snapshots
{
    public abstract class Snapshot : BaseObject<GameObject>
    {
        protected GameObject Target { get; private set; }

        protected override void OnDestroy() {
            Target = null;
        }

        protected override void OnCreate(GameObject arg1) {
            Target = arg1;
        }

        public abstract void Take();

        public abstract void Restore();
    }
}