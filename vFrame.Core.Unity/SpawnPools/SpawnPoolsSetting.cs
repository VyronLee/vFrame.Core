//------------------------------------------------------------
//        File:  SpawnPoolsSetting.cs
//       Brief:  Spawn pools setting.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//    Modified:  2019-09-08 23:47
//   Copyright:  Copyright (c) 2019, VyronLee
//============================================================

using UnityEngine;

namespace vFrame.Core.SpawnPools
{
    public static class SpawnPoolsSetting
    {
        public enum PoolObjectHiddenType
        {
            Deactive,
            Position,
        }

        public static int DefaultCapacity = 40;
        public static int DefaultLifeTime = 30 * 60 * 5; // 5min by 30fps
        public static int GCInterval = 600; // 600 frames, 20s by 30fps

        public static Vector3 RootPosition = new Vector3(-1000, -1000, -1000);
        public static PoolObjectHiddenType HiddenType = PoolObjectHiddenType.Deactive;
    }
}