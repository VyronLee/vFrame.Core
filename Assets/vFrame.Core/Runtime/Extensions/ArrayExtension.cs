// ------------------------------------------------------------
//         File: ArrayExtension.cs
//        Brief: Array extension methods for multi-dimensional array traversal
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2019-09-09 16:56
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    public static class ArrayExtensions
    {
        /// <summary>
        ///     Invokes the specified action for each element in the array, supporting multi-dimensional arrays.
        /// </summary>
        /// <param name="array">The target array.</param>
        /// <param name="action">The action to invoke for each element, receiving the array and current position indices.</param>
        public static void ForEach(this Array array, Action<Array, int[]> action) {
            if (array.LongLength == 0) {
                return;
            }

            var walker = new ArrayTraverse(array);
            do {
                action(array, walker.Position);
            }
            while (walker.Step());
        }
    }

    internal readonly struct ArrayTraverse
    {
        private readonly int[] _maxLengths;
        public readonly int[] Position;

        /// <summary>
        ///     Initializes a new array traverser for the given array.
        /// </summary>
        /// <param name="array">The array to traverse.</param>
        public ArrayTraverse(Array array) {
            _maxLengths = new int[array.Rank];
            for (var i = 0; i < array.Rank; ++i) {
                _maxLengths[i] = array.GetLength(i) - 1;
            }

            Position = new int[array.Rank];
        }

        /// <summary>
        ///     Advances to the next position in the array.
        /// </summary>
        /// <returns><c>true</c> if advanced to the next position; <c>false</c> if the end has been reached.</returns>
        public bool Step() {
            for (var i = 0; i < Position.Length; ++i) {
                if (Position[i] < _maxLengths[i]) {
                    Position[i]++;
                    for (var j = 0; j < i; j++) {
                        Position[j] = 0;
                    }

                    return true;
                }
            }

            return false;
        }
    }
}