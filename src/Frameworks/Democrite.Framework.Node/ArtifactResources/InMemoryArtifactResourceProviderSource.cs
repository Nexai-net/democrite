// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.ArtifactResources
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.ArtifactResources;
    using Democrite.Framework.Toolbox.Patterns.Strategy;

    using System;

    /// <summary>
    /// Artefact provider source used to exposed the one record in memory
    /// </summary>
    /// <seealso cref="IArtifactResourceProviderSource" />
    public sealed class InMemoryArtifactResourceProviderSource : ProviderStrategyBaseSource<IArtifactResource, Guid>, IArtifactResourceProviderSource
    {
        #region Methods

        /// <summary>
        /// Adds or update a new artefact
        /// </summary>
        public void AddOrUpdate(IArtifactResource artifactResource)
        {
            base.SafeAddOrReplace(artifactResource.Uid, artifactResource);
        }

        #endregion
    }
}
