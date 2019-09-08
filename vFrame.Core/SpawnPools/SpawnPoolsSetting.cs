using UnityEngine;

namespace vFrame.Core.SpawnPools
{
    public static class SpawnPoolsSetting
    {
        public enum PoolObjectHiddenType
        {
            DEACTIVE,
            POSITION,
        }
        
        public static Vector3 kPoolsRootPosition = new Vector3(-1000, -1000, -1000);
        public static PoolObjectHiddenType kPoolObjectHiddenType = PoolObjectHiddenType.DEACTIVE;
    }
}