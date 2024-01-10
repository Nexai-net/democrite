// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Configurations
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.ArtifactResources;

    /// <summary>
    /// Build in charge to setup artifact resources and how to access it
    /// </summary>
    public interface IDemocriteNodeArtifacResourceBuilder
    {
        /// <summary>
        /// Adds selector to choose when multiple sources provide the same artifact.
        /// </summary>
        IDemocriteNodeArtifacResourceBuilder AddResourceSelector();

        /// <summary>
        /// Adds the source artefact resources provider.
        /// </summary>
        IDemocriteNodeArtifacResourceBuilder AddSourceProvider<TSource>(TSource singletonInstances)
            where TSource : class, IArtifactResourceProviderSource;

        /// <summary>
        /// Adds the source artefact resources provider.
        /// </summary>
        IDemocriteNodeArtifacResourceBuilder AddSourceProvider<TSource>()
            where TSource : class, IArtifactResourceProviderSource;

        /// <summary>
        /// Adds an artifact in local execution.
        /// </summary>
        IDemocriteNodeArtifacResourceBuilder AddLocalArtifact(IArtifactResource artifactResource);
    }
}
