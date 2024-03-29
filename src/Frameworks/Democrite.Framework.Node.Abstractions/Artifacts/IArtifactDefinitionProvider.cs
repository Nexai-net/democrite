﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Artifacts
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Elvex.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// Provider used to get <see cref="ArtifactDefinition"/> from <see cref="IArtifactDefinitionProviderSource"/>
    /// </summary>
    /// <seealso cref="IProviderStrategy{IArtifactResource, Guid}" />
    public interface IArtifactDefinitionProvider : IProviderStrategy<ArtifactDefinition, Guid>
    {
    }
}
