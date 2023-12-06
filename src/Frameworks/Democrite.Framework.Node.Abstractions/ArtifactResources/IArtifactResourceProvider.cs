// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.ArtifactResources
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// Provider used to get <see cref="IArtifactResource"/> from <see cref="IArtifactResourceProviderSource"/>
    /// </summary>
    /// <seealso cref="IProviderStrategy{IArtifactResource, Guid}" />
    public interface IArtifactResourceProvider : IProviderStrategy<IArtifactResource, Guid>
    {
    }
}
