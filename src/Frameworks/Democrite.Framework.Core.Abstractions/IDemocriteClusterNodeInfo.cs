// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions
{
    using Orleans.Runtime;

    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Provide all information related to current node (SILO) in the democrite cluster
    /// </summary>
    public interface IDemocriteClusterNodeInfo
    {
        /// <summary>
        /// Gets the current address.
        /// </summary>
        SiloAddress CurrentAddress { get; }

        /// <summary>
        /// Gets the name of the cluster.
        /// </summary>
        string ClusterName { get; }

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        string NodeName { get; }

        /// <summary>
        /// Gets the current loaded assemblies.
        /// </summary>
        IReadOnlyCollection<Assembly> CurrentLoadedAssemblies { get; }
    }
}
