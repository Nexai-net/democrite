// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Extensions.Docker.Abstractions.Options
{
    using Democrite.Framework.Node.Abstractions.Models;

    using System.ComponentModel;

    /// <summary>
    /// 
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class DockerEnvironementOptions : INodeOptions
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerEnvironementOptions"/> class.
        /// </summary>
        public DockerEnvironementOptions()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerEnvironementOptions"/> class.
        /// </summary>
        public DockerEnvironementOptions(bool preLoadAllDependenciesImages)
        {
            this.PreLoadAllDependenciesImages = preLoadAllDependenciesImages;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether pre-load all dependencies images at start.
        /// </summary>
        public bool PreLoadAllDependenciesImages { get; }

        #endregion
    }
}
