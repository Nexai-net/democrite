// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Artifacts
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.Artifacts;
    using Democrite.Framework.Toolbox.Patterns.Strategy;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Provider implementatation using all the <see cref="IArtifactProviderSource"/> register
    /// </summary>
    /// <seealso cref="IArtifactProvider" />
    public sealed class ArtifactsProvider : ProviderStrategyBase<ArtifactDefinition, Guid, IArtifactProviderSource>, IArtifactProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactsProvider"/> class.
        /// </summary>
        public ArtifactsProvider(IEnumerable<IArtifactProviderSource> artifactResourceProviderSources,
                                        ILogger<ArtifactsProvider> logger)
            : base(artifactResourceProviderSources, logger)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the fetch by key expressions.
        /// </summary>
        protected override Expression<Func<ArtifactDefinition, bool>> GetFetchByKeyExpressions(IReadOnlyCollection<Guid> keys)
        {
            return (t => keys.Contains(t.Uid));
        }

        #endregion
    }
}
