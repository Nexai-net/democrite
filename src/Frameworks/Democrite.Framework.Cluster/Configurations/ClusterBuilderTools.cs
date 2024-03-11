// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Cluster.Configurations
{
    using Democrite.Framework.Cluster.Abstractions.Services;
    using Democrite.Framework.Cluster.Services;
    using Democrite.Framework.Configurations;
    using Elvex.Toolbox.Abstractions.Services;
    using Elvex.Toolbox.Services;

    /// <summary>
    /// Container about some services used during the cluster build process (node/client)
    /// </summary>
    public sealed class ClusterBuilderTools : IClusterBuilderTools
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ClusterBuilderTools"/> class.
        /// </summary>
        static ClusterBuilderTools()
        {
            Default = new ClusterBuilderTools(new AssemblyInspector(),
                                              new AssemblyLoader(),
                                              new FileSystemHandler(),
                                              new NetworkInspector(),
                                              HashSHA256Service.Instance);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterBuilderTools"/> class.
        /// </summary>
        public ClusterBuilderTools(IAssemblyInspector assemblyInspector,
                                   IAssemblyLoader assemblyLoader,
                                   IFileSystemHandler fileSystemHandler,
                                   INetworkInspector networkInspector,
                                   IHashService hashService)
        {
            this.AssemblyInspector = assemblyInspector;
            this.AssemblyLoader = assemblyLoader;
            this.FileSystemHandler = fileSystemHandler;
            this.NetworkInspector = networkInspector;
            this.HashService = hashService;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the assembly inspector services.
        /// </summary>
        public IAssemblyInspector AssemblyInspector { get; }

        /// <summary>
        /// Gets the assembly loader.
        /// </summary>
        public IAssemblyLoader AssemblyLoader { get; }

        /// <summary>
        /// Gets the file system handler.
        /// </summary>
        public IFileSystemHandler FileSystemHandler { get; }

        /// <summary>
        /// Gets the network inspector.
        /// </summary>
        public INetworkInspector NetworkInspector { get; }

        /// <summary>
        /// Gets the default hash service.
        /// </summary>
        public IHashService HashService { get; }

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static ClusterBuilderTools Default { get; }

        #endregion
    }
}
