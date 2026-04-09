// ------------------------------------------------------------
//         File: BoxAllocator.cs
//        Brief: Pool allocator for <see cref="Box{T}"/> instances.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-20 16:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using vFrame.Core;

namespace vFrame.Core
{
    public class BoxAllocator<T> : IPoolObjectAllocator<Box<T>>
    {
        /// <summary>
        ///     Allocates a new <see cref="Box{T}"/> instance.
        /// </summary>
        /// <returns>A new boxed value instance.</returns>
        public Box<T> Alloc() {
            return new Box<T>();
        }

        /// <summary>
        ///     Resets the boxed value to its default state.
        /// </summary>
        /// <param name="obj">The box to reset.</param>
        public void Reset(Box<T> obj) {
            obj.Value = default;
        }
    }
}
