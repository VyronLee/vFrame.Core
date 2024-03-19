// ------------------------------------------------------------
//         File: AssetLoadFailedException.cs
//        Brief: AssetLoadFailedException.cs
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-3-17 22:53
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core.Unity.SpawnPools
{
    public class AssetLoadFailedException : SpawnPoolException
    {
        public AssetLoadFailedException(string path) : base(path) { }
    }
}