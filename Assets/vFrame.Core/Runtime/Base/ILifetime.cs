namespace vFrame.Core.Base
{
    /// <summary>
    /// Lightweight grouped-cleanup primitive for retained ownership boundaries.
    /// A lifetime can own child lifetimes, destroyables, and cleanup actions, but it is
    /// not intended to grow into a container or orchestration framework.
    /// </summary>
    public interface ILifetime : IDestroyable
    {
        /// <summary>
        /// Creates a child lifetime that ends when this lifetime ends.
        /// </summary>
        ILifetime CreateChild();

        /// <summary>
        /// Binds a destroyable to this lifetime so it is terminated when the lifetime ends.
        /// </summary>
        void Add(IDestroyable destroyable);

        /// <summary>
        /// Binds a cleanup action to this lifetime so it executes when the lifetime ends.
        /// </summary>
        void Add(System.Action action);
    }
}
