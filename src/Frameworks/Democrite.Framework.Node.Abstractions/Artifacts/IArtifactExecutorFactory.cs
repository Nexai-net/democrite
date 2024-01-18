// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Artifacts
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Artifacts;

    using Microsoft.Extensions.Logging;

    using System.Threading.Tasks;

    /// <summary>
    /// Factory in charge to managed link between static information <see cref="IArtifactCodePackageResourceDefinition"/> and 
    /// remote guided execution through <see cref="IArtifactExternalCodeExecutor"/>
    /// </summary>
    public interface IArtifactExecutorFactory
    {
        /// <summary>
        /// Build <see cref="IArtifactExternalCodeExecutor"/> from <see cref="IArtifactCodePackageResourceDefinition"/>
        /// </summary>
        ValueTask<IArtifactExternalCodeExecutor> BuildAsync(ArtifactExecutableDefinition artifactExecutableDefinition,
                                                            IExecutionContext executionContext,
                                                            ILogger logger,
                                                            CancellationToken token);
    }
}
