// ------------------------------------------------------------
//         File: DefaultGameObjectLoaderFactory.cs
//        Brief: Factory that creates DefaultGameObjectLoader instances.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-01-01 00:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core.Unity
{
    internal class DefaultGameObjectLoaderFactory : IGameObjectLoaderFactory
    {
        /// <summary>
        /// Creates and initializes a new <see cref="DefaultGameObjectLoader"/> for the given asset path.
        /// </summary>
        /// <param name="assetPath">The resource path used to load the GameObject prefab.</param>
        /// <returns>An initialized <see cref="IGameObjectLoader"/> ready to load assets.</returns>
        public IGameObjectLoader CreateLoader(string assetPath) {
            var ret = new DefaultGameObjectLoader();
            ret.Create(assetPath);
            return ret;
        }
    }
}
