//------------------------------------------------------------
//        File:  ISpawnPools.cs
//       Brief:  Spawn pools interface.
//
//      Author:  VyronLee, lwz_jz@hotmail.com
//
//     Created:  2019-09-08 23:47
//   Copyright:  Copyright (c) 2024, VyronLee
//============================================================

namespace vFrame.Core.Unity.SpawnPools
{
    public interface ISpawnPools
    {
        IPool this[string assetName] { get; }
        void Update();
        IPreloadAsyncRequest PreloadAsync(string[] assetPaths);
    }
}