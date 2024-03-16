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
using vFrame.Core.Loggers;

namespace vFrame.Core.SpawnPools
{
    public class SpawnPoolsSetting
    {
        public enum PoolObjectHiddenType
        {
            Deactive,
            Position,
        }
        
        private static readonly SpawnPoolsSetting _default = new SpawnPoolsSetting();

        public const int DefaultCapacity = 40;
        public const int DefaultLifeTime = 30 * 60 * 5; // 5min by 30fps
        public const int DefaultGCInterval = 600; // 600 frames, 20s by 30fps
        public static readonly Vector3 DefaultRootPosition = new Vector3(-1000, -1000, -1000);
        public const int DefaultAsyncUploadCount = int.MaxValue;

        public int Capacity { get; set; } = DefaultCapacity;
        public int LifeTime { get; set; } = DefaultLifeTime;
        public int GCInterval { get; set; } = DefaultGCInterval;
        public int AsyncUploadCount { get; set; } = DefaultAsyncUploadCount;
        public Vector3 RootPosition { get; set; } = DefaultRootPosition;
        public PoolObjectHiddenType HiddenType { get; set; } = PoolObjectHiddenType.Deactive;

        public static SpawnPoolsSetting Default => _default;
        public static LogTag LogTag = new LogTag("SpawnPools");
    }
}