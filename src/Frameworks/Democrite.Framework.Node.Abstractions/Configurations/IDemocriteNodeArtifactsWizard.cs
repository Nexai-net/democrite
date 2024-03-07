// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Configurations
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.Artifacts;

    /// <summary>
    /// Build in charge to setup artifact resources and how to access it
    /// </summary>
    public interface IDemocriteNodeArtifactsWizard
    {
        ///// <summary>
        ///// Adds selector to choose when multiple sources provide the same artifact.
        ///// </summary>
        //IDemocriteNodeArtifactsWizard AddResourceSelector();

        /// <summary>
        /// Adds the source artefact resources provider.
        /// </summary>
        IDemocriteNodeArtifactsWizard AddSourceProvider<TSource>(TSource singletonInstances)
            where TSource : class, IArtifactDefinitionProviderSource;

        /// <summary>
        /// Adds the source artefact resources provider.
        /// </summary>
        IDemocriteNodeArtifactsWizard AddSourceProvider<TSource>()
            where TSource : class, IArtifactDefinitionProviderSource;

        /// <summary>
        /// Adds an artifact in definition execution.
        /// </summary>
        IDemocriteNodeArtifactsWizard Register(params ArtifactDefinition[] artifactResource);
    }
}
