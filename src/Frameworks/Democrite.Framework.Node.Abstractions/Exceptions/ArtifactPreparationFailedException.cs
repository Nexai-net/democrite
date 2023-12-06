// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Node.Abstractions.Exceptions
{
    using Democrite.Framework.Core.Abstractions;
    using Democrite.Framework.Core.Abstractions.Exceptions;
    using Democrite.Framework.Node.Abstractions.ArtifactResources;
    using Democrite.Framework.Node.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when <see cref="IExternalCodeExecutorPreparationStep"/> setup/unstep failed
    /// </summary>
    /// <seealso cref="DemocriteExecutionBaseException" />
    public sealed class ArtifactPreparationFailedException : DemocriteExecutionBaseException
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactPreparationFailedException"/> class.
        /// </summary>
        public ArtifactPreparationFailedException(IExternalCodeExecutorPreparationStep externalCodeExecutorPreparationStep,
                                                  IExecutionContext executionContext,
                                                  Exception? innerException = null)
            : base(NodeAbstractionExceptionSR.ArtifactPreparationFailed.WithArguments(externalCodeExecutorPreparationStep),
                   executionContext,
                   DemocriteErrorCodes.Build(DemocriteErrorCodes.Categories.Artifact,
                                             DemocriteErrorCodes.PartType.Setup,
                                             DemocriteErrorCodes.ErrorType.Failed),
                   innerException)
        {
            this.Data.Add(nameof(externalCodeExecutorPreparationStep), externalCodeExecutorPreparationStep.ToString());
        }

        #endregion
    }
}
