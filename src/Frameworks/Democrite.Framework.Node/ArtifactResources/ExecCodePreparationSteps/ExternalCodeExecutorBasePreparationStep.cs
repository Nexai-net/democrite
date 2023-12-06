// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.ArtifactResources.ExecCodePreparationSteps
{
    using Democrite.Framework.Core.Abstractions.Artifacts;
    using Democrite.Framework.Node.Abstractions.ArtifactResources;

    using Microsoft.Extensions.Logging;

    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base implementation of <see cref="IExternalCodeExecutorPreparationStep"/> to allow child to override only necessary setup and unsetup part.
    /// </summary>
    /// <seealso cref="IExternalCodeExecutorPreparationStep" />
    public abstract class ExternalCodeExecutorBasePreparationStep : IExternalCodeExecutorPreparationStep
    {
        #region Methods

        /// <inheritdoc />
        public virtual ValueTask<bool> SetupAsync(IArtifactCodePackageResource artifactCodePackageResource, ILogger logger, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(true);
        }

        /// <inheritdoc />
        public virtual ValueTask<bool> UnsetupAsync(IArtifactCodePackageResource artifactCodePackageResource, ILogger logger, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(true);
        }

        #endregion
    }
}
