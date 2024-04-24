// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Artifacts
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Dedicated factory to build executor from definition
    /// </summary>
    public interface IArtifactExecutorDedicatedFactory
    {
        /// <summary>
        /// Determines whether this instance can managed the specified artifact executable definition.
        /// </summary>
        bool CanManaged(ArtifactExecutableDefinition artifactExecutableDefinition);

        /// <summary>
        /// Checks the cached executor validity.
        /// </summary>
        ValueTask<bool> CheckExecutorValidityAsync(ArtifactExecutableDefinition artifactExecutableDefinition,
                                                   IArtifactExternalCodeExecutor artifact,
                                                   IExecutionContext executionContext,
                                                   ILogger logger,
                                                   CancellationToken token);

        /// <summary>
        /// Builds a new artifact executor.
        /// </summary>
        ValueTask<IArtifactExternalCodeExecutor> BuildNewExecutorAsync(ArtifactExecutableDefinition artifactExecutableDefinition,
                                                                       IArtifactExternalCodeExecutor? previousExecutor,
                                                                       IExecutionContext executionContext,
                                                                       ILogger logger,
                                                                       CancellationToken token);
    }
}
