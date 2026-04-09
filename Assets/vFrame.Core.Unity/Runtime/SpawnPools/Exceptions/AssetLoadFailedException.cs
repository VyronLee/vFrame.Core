// ------------------------------------------------------------
//         File: AssetLoadFailedException.cs
//        Brief: Exception thrown when an asset cannot be loaded from the specified resource path.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-17 22:53:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core.Unity
{
    public class AssetLoadFailedException : SpawnPoolException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetLoadFailedException"/> class with the failed resource path.
        /// </summary>
        /// <param name="path">The resource path that failed to load.</param>
        public AssetLoadFailedException(string path) : base(path) { }
    }
}
