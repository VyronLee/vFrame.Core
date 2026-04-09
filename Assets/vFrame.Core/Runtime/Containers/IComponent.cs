// ------------------------------------------------------------
//         File: IComponent.cs
//        Brief: Interface for a component that can be bound to a container
//
//       Author: VyronLee, lwz_jz@hotmail.com
//
//      Created: 2016-09-21 19:18:00
//    Copyright: Copyright (c) 2024, VyronLee
// ============================================================

namespace vFrame.Core
{
    public interface IComponent
    {
        /// <summary>
        /// Gets the container to which this component is bound.
        /// </summary>
        /// <returns>The owning container instance.</returns>
        IContainer GetContainer();
    }
}
