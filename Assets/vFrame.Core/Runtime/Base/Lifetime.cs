using System;
using System.Collections.Generic;
using vFrame.Core.Exceptions;

namespace vFrame.Core.Base
{
    /// <summary>
    /// Lightweight grouped-cleanup implementation used for child ownership and
    /// same-lifetime resource binding.
    /// </summary>
    public sealed class Lifetime : ILifetime
    {
        private List<IDestroyable> _destroyables;
        private List<Action> _actions;

        public bool Destroyed { get; private set; }

        /// <summary>
        /// Creates a child lifetime that is destroyed through this lifetime's cleanup boundary.
        /// </summary>
        public ILifetime CreateChild() {
            var lifetime = new Lifetime();
            Add(lifetime);
            return lifetime;
        }

        /// <summary>
        /// Binds a destroyable resource to this lifetime.
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
        /// Binds a cleanup action to this lifetime.
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
        /// Ends the lifetime and tears down child lifetimes, destroyables, and cleanup actions
        /// through one grouped cleanup boundary.
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

        public void Dispose() {
            Destroy();
        }
    }
}
