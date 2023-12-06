// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Artifacts
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Toolbox.Abstractions.Supports;

    using System;

    /// <summary>
    /// Base implementation of <see cref="IArtifactResource"/>
    /// </summary>
    /// <seealso cref="IArtifactResource" />
    public abstract class ArtifactBaseResource : IArtifactResource, ISupportDebugDisplayName
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactBaseResource"/> class.
        /// </summary>
        protected ArtifactBaseResource(Guid id,
                                       string displayName,
                                       string? description,
                                       Version version,
                                       string hash,
                                       DateTime creationOn,
                                       ArtifactResourceTypeEnum artifactResourceType)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(displayName);
            ArgumentNullException.ThrowIfNull(version);
            ArgumentNullException.ThrowIfNull(hash);
            ArgumentNullException.ThrowIfNull(creationOn);

            this.Uid = id;
            this.DisplayName = displayName;
            this.Description = description;
            this.Version = version;
            this.Hash = hash;
            this.CreationOn = creationOn;
            this.Type = artifactResourceType;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public Guid Uid { get; }

        /// <inheritdoc />
        public virtual string DisplayName { get; }

        /// <inheritdoc />
        public string? Description { get; }

        /// <inheritdoc />
        public Version Version { get; }

        /// <inheritdoc />
        public string Hash { get; }

        /// <inheritdoc />
        public DateTime CreationOn { get; }

        /// <inheritdoc />
        public ArtifactResourceTypeEnum Type { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual string ToDebugDisplayName()
        {
            return $"{this.Uid}-{this.DisplayName}-{this.Version}-{this.Description?.Take(21)}";
        }

        #endregion
    }
}
