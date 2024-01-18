// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Artifacts
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.Artifacts;
    using Democrite.Framework.Toolbox.Patterns.Strategy;

    using System;

    /// <summary>
    /// Artefact provider source used to exposed the one record in memory
    /// </summary>
    /// <seealso cref="IArtifactProviderSource" />
    public sealed class InMemoryArtifactProviderSource : ProviderStrategyBaseSource<ArtifactDefinition, Guid>, IArtifactProviderSource
    {
        #region Methods

        /// <summary>
        /// Adds or update a new artefact
        /// </summary>
        public void AddOrUpdate(ArtifactDefinition artifactResource)
        {
            base.SafeAddOrReplace(artifactResource.Uid, artifactResource);
        }

        #endregion
    }
}
