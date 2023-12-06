// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.ArtifactResources
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.ArtifactResources;
    using Democrite.Framework.Toolbox.Patterns.Strategy;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Provider implementatation using all the <see cref="IArtifactResourceProviderSource"/> register
    /// </summary>
    /// <seealso cref="IArtifactResourceProvider" />
    public sealed class ArtifactResourceProvider : ProviderStrategyBase<IArtifactResource, Guid, IArtifactResourceProviderSource>, IArtifactResourceProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactResourceProvider"/> class.
        /// </summary>
        public ArtifactResourceProvider(IEnumerable<IArtifactResourceProviderSource> artifactResourceProviderSources,
                                        ILogger<ArtifactResourceProvider> logger)
            : base(artifactResourceProviderSources, logger)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the fetch by key expressions.
        /// </summary>
        protected override Expression<Func<IArtifactResource, bool>> GetFetchByKeyExpressions(IReadOnlyCollection<Guid> keys)
        {
            return (t => keys.Contains(t.Uid));
        }

        #endregion
    }
}
