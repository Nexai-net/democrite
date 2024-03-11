// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Cluster.Abstractions.Services;
    using Elvex.Toolbox.Abstractions.Services;

    /// <summary>
    /// Contains all tools needed during the democrite cluster part setup
    /// </summary>
    public interface IClusterBuilderTools
    {
        /// <summary>
        /// Gets the assembly inspector services.
        /// </summary>
        IAssemblyInspector AssemblyInspector { get; }

        /// <summary>
        /// Gets the assembly loader.
        /// </summary>
        IAssemblyLoader AssemblyLoader { get; }

        /// <summary>
        /// Gets the file system handler.
        /// </summary>
        IFileSystemHandler FileSystemHandler { get; }

        /// <summary>
        /// Gets the network inspector.
        /// </summary>
        INetworkInspector NetworkInspector { get; }
    }
}
