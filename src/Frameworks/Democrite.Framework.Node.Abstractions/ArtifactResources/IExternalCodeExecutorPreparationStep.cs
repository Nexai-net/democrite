// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.ArtifactResources
{
    using Democrite.Framework.Core.Abstractions.Artifacts;

    using Microsoft.Extensions.Logging;

    using System.Threading.Tasks;

    /// <summary>
    /// Preparation step used to setup and unsetup the code execution context
    /// </summary>
    public interface IExternalCodeExecutorPreparationStep
    {
        /// <summary>
        /// Prepare the <paramref name="artifactCodePackageResource"/> to be ready for execution
        /// </summary>
        /// <returns>
        ///     <c>True</c> if the setup work; otherwise false if fatal error occured and process MUST stop.
        /// </returns>
        ValueTask<bool> SetupAsync(IArtifactCodePackageResource artifactCodePackageResource, ILogger logger, CancellationToken cancellationToken);

        /// <summary>
        /// Clean up <paramref name="artifactCodePackageResource"/>
        /// </summary>
        /// <returns>
        ///     <c>True</c> if the setup work; otherwise false if fatal error occured and process MUST stop.
        /// </returns>
        ValueTask<bool> UnsetupAsync(IArtifactCodePackageResource artifactCodePackageResource, ILogger logger, CancellationToken cancellationToken);
    }
}
