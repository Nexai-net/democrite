// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Artifacts
{
    using Democrite.Framework.Core;
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.Artifacts;

    using Microsoft.Extensions.Logging;

    using System.Collections.Generic;

    /// <summary>
    /// Provider implementatation using all the <see cref="IArtifactDefinitionProviderSource"/> register
    /// </summary>
    /// <seealso cref="IArtifactDefinitionProvider" />
    internal sealed class ArtifactDefinitionProvider : DefinitionBaseProvider<ArtifactDefinition, IArtifactDefinitionProviderSource>, IArtifactDefinitionProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactsProvider"/> class.
        /// </summary>
        public ArtifactDefinitionProvider(IEnumerable<IArtifactDefinitionProviderSource> specificDefinitionProviderSources,
                                           IEnumerable<IDefinitionSourceProvider<ArtifactDefinition>> genericDefinitionSourceProviders,
                                           ILogger<IArtifactDefinitionProviderSource> logger) 
            : base(specificDefinitionProviderSources, genericDefinitionSourceProviders, logger)
        {
        }

        #endregion
    }
}
