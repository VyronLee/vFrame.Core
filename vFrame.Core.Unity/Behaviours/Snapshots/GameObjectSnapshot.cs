using UnityEngine;

namespace vFrame.Core.Behaviours.Snapshots
{
    public abstract class GameObjectSnapshot : MonoBehaviour
    {
        public abstract bool Take();

        public abstract bool Restore();
    }
}