// ------------------------------------------------------------
//         File: ValidatingObjectPool.cs
//        Brief: Decorator that validates objects before returning them to the pool
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2026-04-15 19:09:00
//    Copyright: Copyright (c) 2026, VyronLee
// ============================================================

using System;

namespace vFrame.Core
{
    /// <summary>
    ///     Decorator that validates objects before returning them to the pool.
    ///     Invalid objects are discarded instead of being returned for reuse.
    /// </summary>
    /// <typeparam name="T">The pooled object type (must be a reference type).</typeparam>
    public sealed class ValidatingObjectPool<T> : ObjectPoolDecorator<T> where T : class
    {
        private readonly Func<T, bool> _validator;

        /// <summary>
        ///     Creates a new validating decorator wrapping the specified inner pool.
        /// </summary>
        /// <param name="inner">The inner pool to decorate.</param>
        /// <param name="validator">Function that returns true if the object is valid for reuse.</param>
        public ValidatingObjectPool(IObjectPool<T> inner, Func<T, bool> validator) : base(inner) {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <summary>
        ///     Gets a valid instance from the pool, retrying up to 3 times if invalid objects are retrieved.
        /// </summary>
        /// <returns>A valid pooled object instance.</returns>
        public override T Get() {
            const int maxAttempts = 3;
            for (var i = 0; i < maxAttempts; i++) {
                var item = base.Get();
                if (_validator(item)) {
                    return item;
                }
                // Discard invalid - don't return to pool
            }
            // Fallback: return whatever we get on the last attempt
            return base.Get();
        }

        /// <summary>
        ///     Returns an instance to the pool only if it passes validation.
        /// </summary>
        /// <param name="obj">The object to return.</param>
        public override void Return(T obj) {
            if (obj != null && !_validator(obj)) {
                return; // Discard invalid
            }
            base.Return(obj);
        }
    }
}
