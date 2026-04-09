// ------------------------------------------------------------
//         File: Box.cs
//        Brief: A lightweight value wrapper backed by object pooling.
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-20 16:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using vFrame.Core;

namespace vFrame.Core
{
    public class Box<T> : BaseObject<T>
    {
        public T Value;

        /// <summary>
        ///     Stores the initial value during creation.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        protected override void OnCreate(T value) {
            Value = value;
        }

        /// <summary>
        ///     Resets the boxed value to default.
        /// </summary>
        protected override void OnDestroy() {
            Value = default;
        }
    }
}
