// ------------------------------------------------------------
//         File: IGameObjectLoaderFactory.cs
//        Brief: Factory interface for creating GameObject loaders by asset path.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core.Unity
{
    /// <summary>
    /// Factory that creates <see cref="IGameObjectLoader"/> instances for a given asset path.
    /// </summary>
    public interface IGameObjectLoaderFactory
    {
        /// <summary>
        /// Creates a new <see cref="IGameObjectLoader"/> for the specified asset path.
        /// </summary>
        /// <param name="assetPath">The path of the asset to load.</param>
        /// <returns>A new loader instance ready to load the asset.</returns>
        IGameObjectLoader CreateLoader(string assetPath);
    }
}
