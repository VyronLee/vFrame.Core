// ------------------------------------------------------------
//         File: Lifetime.cs
//        Brief: Concrete lifetime for grouped cleanup of resources
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2024-03-18 16:00:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

using System;
using System.Collections.Generic;

namespace vFrame.Core
{
    /// <summary>
    ///     Lightweight grouped-cleanup implementation used for child ownership and
    ///     same-lifetime resource binding.
    /// </summary>
    public sealed class Lifetime : ILifetime
    {
        private List<Action> _actions;
        private List<IDestroyable> _destroyables;

        /// <summary>
        ///     Gets whether the lifetime has been destroyed.
        /// </summary>
        public bool Destroyed { get; private set; }

        /// <summary>
        ///     Creates a child lifetime that is destroyed through this lifetime's cleanup boundary.
        /// </summary>
        public ILifetime CreateChild() {
            var lifetime = new Lifetime();
            Add(lifetime);
            return lifetime;
        }

        /// <summary>
        ///     Binds a destroyable resource to this lifetime.
        /// </summary>
        public void Add(IDestroyable destroyable) {
            ThrowHelper.ThrowIfNull(destroyable, nameof(destroyable));

            if (Destroyed) {
                destroyable.Destroy();
                return;
            }

            _destroyables ??= new List<IDestroyable>();
            _destroyables.Add(destroyable);
        }

        /// <summary>
        ///     Binds a cleanup action to this lifetime.
        /// </summary>
        public void Add(Action action) {
            ThrowHelper.ThrowIfNull(action, nameof(action));

            if (Destroyed) {
                action();
                return;
            }

            _actions ??= new List<Action>();
            _actions.Add(action);
        }

        /// <summary>
        ///     Ends the lifetime and tears down child lifetimes, destroyables, and cleanup actions
        ///     through one grouped cleanup boundary.
        /// </summary>
        public void Destroy() {
            if (Destroyed) {
                return;
            }

            try {
                Destroyed = true;

                if (_destroyables != null) {
                    for (var index = _destroyables.Count - 1; index >= 0; index--) {
                        _destroyables[index]?.Destroy();
                    }
                }

                if (_actions != null) {
                    for (var index = _actions.Count - 1; index >= 0; index--) {
                        _actions[index]?.Invoke();
                    }
                }
            }
            finally {
                _destroyables?.Clear();
                _destroyables = null;

                _actions?.Clear();
                _actions = null;
            }
        }

        /// <summary>
        ///     Releases all resources by invoking <see cref="Destroy" />.
        /// </summary>
        public void Dispose() {
            Destroy();
        }
    }
}