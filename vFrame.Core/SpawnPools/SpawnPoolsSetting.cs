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
        
        public static Vector3 RootPosition = new Vector3(-1000, -1000, -1000);
        public static PoolObjectHiddenType HiddenType = PoolObjectHiddenType.Deactive;
    }
}