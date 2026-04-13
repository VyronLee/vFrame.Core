// ------------------------------------------------------------
//         File: IPoolObjectAllocator.cs
//        Brief: Pool allocator interface defining creation and reset strategies for pooled objects
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-07-09 19:19:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    public interface IPoolObjectAllocator<T>
    {
        /// <summary>
        ///     Creates a new instance of <typeparamref name="T" />.
        /// </summary>
        /// <returns>A newly allocated instance.</returns>
        T Alloc();

        /// <summary>
        ///     Resets the given instance so it can be safely reused.
        /// </summary>
        /// <param name="obj">The instance to reset.</param>
        void Reset(T obj);
    }
}