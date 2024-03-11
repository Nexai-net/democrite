// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Core.Abstractions.Artifacts
{
    using Elvex.Toolbox.Abstractions.Supports;

    using Microsoft.Extensions.Logging;

    using System;

    /// <summary>
    /// Base implementation of <see cref="IArtifactResourceDefinition"/>
    /// </summary>
    /// <seealso cref="IArtifactResourceDefinition" />
    public abstract class ArtifactDefinition : ISupportDebugDisplayName, IDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactDefinition"/> class.
        /// </summary>
        protected ArtifactDefinition(Guid uid,
                                     string displayName,
                                     string? description,
                                     Version? version,
                                     string hash,
                                     DateTime creationOn,
                                     ArtifactTypeEnum artifactType)
        {
            ArgumentNullException.ThrowIfNull(uid);
            ArgumentNullException.ThrowIfNull(displayName);
            ArgumentNullException.ThrowIfNull(hash);
            ArgumentNullException.ThrowIfNull(creationOn);

            this.Uid = uid;
            this.DisplayName = displayName;
            this.Description = description;
            this.Version = version;
            this.Hash = hash;
            this.CreationOn = creationOn;
            this.ArtifactType = artifactType;
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
        public Version? Version { get; }

        /// <inheritdoc />
        public string Hash { get; }

        /// <inheritdoc />
        public DateTime CreationOn { get; }

        /// <inheritdoc />
        public ArtifactTypeEnum ArtifactType { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual string ToDebugDisplayName()
        {
            return $"{this.Uid}-{this.DisplayName}-{this.Version}-{this.Description?.Take(21)}";
        }

        /// <inheritdoc />
        public bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
