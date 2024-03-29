//------------------------------------------------------------
//        File:  SpawnPoolsSetting.cs
//       Brief:  Spawn pools setting.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-08 23:47
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

using UnityEngine;
using vFrame.Core.Loggers;

namespace vFrame.Core.Unity.SpawnPools
{
    public class SpawnPoolsSettings
    {
        public static LogTag LogTag = new LogTag("SpawnPools");
        public int Capacity { get; set; } = 40;
        public int LifeTime { get; set; } = 30 * 60 * 5; // 5min by 30fps
        public int GCInterval { get; set; } = 600; // 600 frames, 20s by 30fps
        public Vector3 RootPosition { get; set; } = new Vector3(-1000, -1000, -1000);

        public static SpawnPoolsSettings Default { get; } = new SpawnPoolsSettings();
    }
}