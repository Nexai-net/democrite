﻿// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.ArtifactResources
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Toolbox.Abstractions.Patterns.Strategy;

    /// <summary>
    /// Define an artifact source provider
    /// </summary>
    public interface IArtifactResourceProviderSource : IProviderStrategySource<IArtifactResource, Guid>
    {
    }
}
